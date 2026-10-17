import { InjectionToken } from '@angular/core';
import { VideoPlayerErrorCode, VideoPlayerState } from './types';

/** The part of an hls.js instance the on-demand session uses. */
export interface HlsLike {
  levels: readonly { height: number }[];
  loadSource(url: string): void;
  attachMedia(video: HTMLVideoElement): void;
  on(event: string, handler: (event: string, data: unknown) => void): void;
  destroy(): void;
}

/** Creates the hls.js instance for one player; tests replace it with a fake. */
export type HlsFactory = () => Promise<HlsLike>;

interface HlsConstructor {
  new (config: { enableWorker: boolean }): HlsLike;
  isSupported(): boolean;
}

/** Lazily imports the hls.js light build so it never reaches the initial bundle. */
export const HLS_FACTORY = new InjectionToken<HlsFactory>('HLS_FACTORY', {
  providedIn: 'root',
  factory: () => async () => {
    // @ts-expect-error -- hls.js ships no typings for its light build; the API matches the full build.
    const module: { default: HlsConstructor } = await import('hls.js/dist/hls.light.mjs');
    const Hls = module.default;
    if (!Hls.isSupported()) throw new Error('MediaSource is not available');
    return new Hls({ enableWorker: false });
  },
});

/** What the on-demand session needs from the component that hosts it. */
export interface HlsVodHost {
  setState(state: VideoPlayerState): void;
  state(): VideoPlayerState;
  fail(code: VideoPlayerErrorCode, cause?: unknown): void;
  videoElement(): HTMLVideoElement;
  requestPlay(): void;
  /** Current position and duration in seconds, as reported by the media element. */
  timeChanged(currentTime: number, duration: number): void;
  /** Height of the active rendition, or `null` when the browser chooses (native HLS). */
  qualityChanged(height: number | null): void;
  /** The signed link no longer works; the host should supply a fresh `src`. */
  linkExpired(): void;
}

const HLS_MIME = 'application/vnd.apple.mpegurl';
const VIDEO_EVENTS = [
  'loadstart',
  'loadedmetadata',
  'durationchange',
  'timeupdate',
  'waiting',
  'playing',
  'pause',
  'ended',
  'error',
] as const;

interface HlsErrorData {
  fatal: boolean;
  type: string;
  response?: { code?: number };
}

/** Owns one on-demand lifecycle against the host `<video>`: native HLS or hls.js. */
export class HlsVodSession {
  private hls: HlsLike | undefined;
  private src = '';
  private native = false;
  private disposed = false;
  /** Metadata of the current source has loaded, so a refused request means an expired link. */
  private started = false;
  private linkExpiredEmitted = false;
  private resumeAt: number | undefined;
  private readonly listener = (event: Event) =>
    this.onVideoEvent(event.type as (typeof VIDEO_EVENTS)[number]);

  constructor(
    private readonly host: HlsVodHost,
    private readonly factory: HlsFactory,
  ) {}

  /** Attaches the host video to `src`; called once per session. */
  async attach(src: string): Promise<void> {
    this.src = src;
    this.host.setState('connecting');
    const video = this.host.videoElement();
    for (const type of VIDEO_EVENTS) video.addEventListener(type, this.listener);
    if (video.canPlayType(HLS_MIME)) {
      this.native = true;
      this.host.qualityChanged(null);
      video.src = this.src;
      return;
    }
    let hls: HlsLike;
    try {
      hls = await this.factory();
    } catch (cause) {
      if (!this.disposed) this.host.fail('unsupported', cause);
      return;
    }
    if (this.disposed) return hls.destroy();
    this.hls = hls;
    hls.on('hlsError', (_event, data) => this.onHlsError(data as HlsErrorData));
    hls.on('hlsLevelSwitched', (_event, data) =>
      this.host.qualityChanged(hls.levels[(data as { level: number }).level]?.height ?? null),
    );
    hls.attachMedia(video);
    hls.loadSource(this.src);
  }

  /** Loads a fresh source and, when `resumeAt` is positive, returns to that position. */
  loadSource(src: string, resumeAt?: number): void {
    this.src = src;
    this.resumeAt = resumeAt;
    this.started = false;
    this.linkExpiredEmitted = false;
    if (this.native) this.host.videoElement().src = src;
    else this.hls?.loadSource(src);
  }

  dispose(): void {
    this.disposed = true;
    const video = this.host.videoElement();
    for (const type of VIDEO_EVENTS) video.removeEventListener(type, this.listener);
    this.hls?.destroy();
    if (this.native) video.removeAttribute('src');
  }

  private onVideoEvent(type: (typeof VIDEO_EVENTS)[number]): void {
    const video = this.host.videoElement();
    const state = this.host.state();
    switch (type) {
      case 'loadstart':
        if (!this.started) this.host.setState('connecting');
        break;
      case 'loadedmetadata':
        this.started = true;
        this.reportTime();
        if (this.resumeAt === undefined) return this.host.requestPlay();
        video.currentTime = this.resumeAt;
        this.resumeAt = undefined;
        if (state !== 'paused') video.play().catch(() => undefined);
        break;
      case 'durationchange':
      case 'timeupdate':
        this.reportTime();
        break;
      case 'waiting':
        if (state === 'playing') this.host.setState('buffering');
        break;
      case 'playing':
        this.host.setState('playing');
        break;
      case 'pause':
        if (!video.ended && (state === 'playing' || state === 'buffering'))
          this.host.setState('paused');
        break;
      case 'ended':
        this.host.setState('ended');
        break;
      case 'error':
        this.onVideoError(video.error?.code);
        break;
    }
  }

  private reportTime(): void {
    const video = this.host.videoElement();
    this.host.timeChanged(this.resumeAt ?? video.currentTime, video.duration);
  }

  /** Native HLS exposes no HTTP status: a network failure after playback started is an expired link. */
  private onVideoError(code: number | undefined): void {
    if (this.native && this.started && (code === 2 || code === 4)) return this.expire();
    this.host.fail(code === 3 ? 'decode' : 'source', code);
  }

  private onHlsError(data: HlsErrorData): void {
    const status = data.response?.code;
    if (data.type === 'networkError' && (status === 401 || status === 403 || status === 404)) {
      if (this.started) return this.expire();
      return this.host.fail(status === 404 ? 'not-found' : 'unauthorized', data);
    }
    if (data.fatal) this.host.fail(data.type === 'mediaError' ? 'decode' : 'source', data);
  }

  private expire(): void {
    if (this.linkExpiredEmitted) return;
    this.linkExpiredEmitted = true;
    this.host.linkExpired();
  }
}
