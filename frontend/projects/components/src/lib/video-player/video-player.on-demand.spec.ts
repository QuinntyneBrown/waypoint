import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { HLS_FACTORY, HlsLike } from './hls-vod-session';
import { VideoPlayerError, VideoPlayerState } from './types';
import { VideoPlayer } from './video-player';

class FakeHls implements HlsLike {
  levels = [{ height: 360 }, { height: 720 }];
  loadSource = vi.fn();
  attachMedia = vi.fn();
  destroy = vi.fn();
  private handlers = new Map<string, (event: string, data: unknown) => void>();
  on(event: string, handler: (event: string, data: unknown) => void): void {
    this.handlers.set(event, handler);
  }
  emit(event: string, data: unknown): void {
    this.handlers.get(event)?.(event, data);
  }
}

@Component({
  imports: [VideoPlayer],
  template: `<ph-video-player
    [src]="src()"
    [streamId]="streamId()"
    poster="/poster.jpg"
    (error)="errors.push($event)"
    (stateChange)="states.push($event)"
    (tokenExpired)="expired = expired + 1"
  />`,
})
class Host {
  src = signal<string | null>('/media/t1/master.m3u8');
  streamId = signal<string | null>(null);
  errors: VideoPlayerError[] = [];
  states: VideoPlayerState[] = [];
  expired = 0;
}

const mediaState = new WeakMap<HTMLMediaElement, { currentTime: number; duration: number }>();
const stateOf = (video: HTMLMediaElement) => {
  let state = mediaState.get(video);
  if (!state) mediaState.set(video, (state = { currentTime: 0, duration: NaN }));
  return state;
};

async function render(options: { native?: boolean } = {}) {
  const hls = new FakeHls();
  const factory = vi.fn(() => Promise.resolve(hls));
  vi.spyOn(HTMLMediaElement.prototype, 'canPlayType').mockReturnValue(
    options.native ? 'maybe' : '',
  );
  TestBed.configureTestingModule({
    imports: [Host],
    providers: [{ provide: HLS_FACTORY, useValue: factory }],
  });
  const fixture: ComponentFixture<Host> = TestBed.createComponent(Host);
  await fixture.whenStable();
  const player = fixture.nativeElement.querySelector('ph-video-player') as HTMLElement;
  const video = player.querySelector('video') as HTMLVideoElement;
  const fire = async (type: string) => {
    video.dispatchEvent(new Event(type));
    await fixture.whenStable();
  };
  const setDuration = async (duration: number) => {
    stateOf(video).duration = duration;
    await fire('durationchange');
  };
  return { fixture, player, video, hls, factory, fire, setDuration };
}

describe('VideoPlayer on demand', () => {
  beforeEach(() => {
    vi.stubGlobal(
      'ResizeObserver',
      class {
        observe() {}
        disconnect() {}
      },
    );
    for (const name of ['currentTime', 'duration'] as const)
      Object.defineProperty(HTMLMediaElement.prototype, name, {
        configurable: true,
        get(this: HTMLMediaElement) {
          return stateOf(this)[name];
        },
        set(this: HTMLMediaElement, value: number) {
          stateOf(this)[name] = value;
        },
      });
    vi.spyOn(HTMLMediaElement.prototype, 'play').mockResolvedValue(undefined);
    vi.spyOn(HTMLMediaElement.prototype, 'pause').mockImplementation(() => undefined);
  });

  afterEach(() => {
    vi.restoreAllMocks();
    vi.unstubAllGlobals();
    for (const name of ['currentTime', 'duration'])
      Reflect.deleteProperty(HTMLMediaElement.prototype, name);
  });

  it('asks the factory to attach the source when src is set', async () => {
    const { player, video, hls, factory } = await render();
    await vi.waitFor(() => expect(hls.loadSource).toHaveBeenCalledWith('/media/t1/master.m3u8'));
    expect(factory).toHaveBeenCalledOnce();
    expect(hls.attachMedia).toHaveBeenCalledWith(video);
    expect(video.getAttribute('poster')).toBe('/poster.jpg');
    expect(player.getAttribute('data-state')).toBe('connecting');
    expect(player.querySelector('[data-control="live"]')).toBeNull();
  });

  it('uses native HLS without calling the factory', async () => {
    const { video, factory, setDuration, player } = await render({ native: true });
    expect(video.getAttribute('src')).toBe('/media/t1/master.m3u8');
    expect(factory).not.toHaveBeenCalled();
    await setDuration(100);
    expect(player.querySelector('.ph-video-player__quality')).toBeNull();
  });

  it('seeks from the range input and with the arrow keys by 5 seconds', async () => {
    const { fixture, player, video, setDuration } = await render();
    await setDuration(100);
    const seek = player.querySelector('[data-control="seek"]') as HTMLInputElement;
    seek.value = '40';
    seek.dispatchEvent(new Event('input'));
    expect(video.currentTime).toBe(40);
    const press = (key: string) =>
      player.dispatchEvent(new KeyboardEvent('keydown', { key, bubbles: true }));
    press('ArrowRight');
    expect(video.currentTime).toBe(45);
    press('ArrowLeft');
    press('ArrowLeft');
    expect(video.currentTime).toBe(35);
    await fixture.whenStable();
  });

  it('shows elapsed and total time and speaks the position', async () => {
    const { player, video, fire, setDuration } = await render();
    await setDuration(4328);
    video.currentTime = 1421;
    await fire('timeupdate');
    expect(player.querySelector('.ph-video-player__time')?.textContent?.trim()).toBe(
      '23:41 / 1:12:08',
    );
    const seek = player.querySelector('[data-control="seek"]') as HTMLInputElement;
    expect(seek.getAttribute('aria-label')).toBe('Seek');
    expect(seek.max).toBe('4328');
    expect(seek.getAttribute('aria-valuetext')).toBe(
      '23 minutes 41 seconds of 1 hour 12 minutes 8 seconds',
    );
  });

  it('labels the quality from the active level', async () => {
    const { fixture, player, hls } = await render();
    await vi.waitFor(() => expect(hls.loadSource).toHaveBeenCalled());
    hls.emit('hlsLevelSwitched', { level: 1 });
    await fixture.whenStable();
    expect(player.querySelector('.ph-video-player__quality')?.textContent?.trim()).toBe('720p');
    hls.emit('hlsLevelSwitched', { level: 0 });
    await fixture.whenStable();
    expect(player.querySelector('.ph-video-player__quality')?.textContent?.trim()).toBe('360p');
  });

  it('maps video events onto the state machine', async () => {
    const { player, fire, setDuration } = await render();
    await setDuration(100);
    await fire('playing');
    expect(player.getAttribute('data-state')).toBe('playing');
    await fire('waiting');
    expect(player.getAttribute('data-state')).toBe('buffering');
    await fire('playing');
    await fire('pause');
    expect(player.getAttribute('data-state')).toBe('paused');
    await fire('ended');
    expect(player.getAttribute('data-state')).toBe('ended');
  });

  it('emits tokenExpired on a 404 and resumes at the same time with the new src', async () => {
    const { fixture, player, video, hls, fire, setDuration } = await render();
    await vi.waitFor(() => expect(hls.loadSource).toHaveBeenCalled());
    await fire('loadedmetadata');
    await setDuration(4328);
    video.currentTime = 1421;
    await fire('timeupdate');
    hls.emit('hlsError', {
      fatal: false,
      type: 'networkError',
      details: 'fragLoadError',
      response: { code: 404 },
    });
    await fixture.whenStable();
    expect(fixture.componentInstance.expired).toBe(1);
    expect(player.querySelector('[role="status"].ph-video-player__renewing')).not.toBeNull();
    expect(fixture.componentInstance.errors).toEqual([]);

    fixture.componentInstance.src.set('/media/t2/master.m3u8');
    await fixture.whenStable();
    expect(hls.loadSource).toHaveBeenLastCalledWith('/media/t2/master.m3u8');
    video.currentTime = 0;
    await fire('loadedmetadata');
    expect(video.currentTime).toBe(1421);
    await fire('playing');
    expect(player.querySelector('.ph-video-player__renewing')).toBeNull();
  });

  it('emits tokenExpired on a 401 as well', async () => {
    const { fixture, hls, fire } = await render();
    await vi.waitFor(() => expect(hls.loadSource).toHaveBeenCalled());
    await fire('loadedmetadata');
    hls.emit('hlsError', {
      fatal: true,
      type: 'networkError',
      details: 'levelLoadError',
      response: { code: 401 },
    });
    expect(fixture.componentInstance.expired).toBe(1);
  });

  it('fails with unauthorized when the first playlist is refused', async () => {
    const { fixture, player, hls } = await render();
    await vi.waitFor(() => expect(hls.loadSource).toHaveBeenCalled());
    hls.emit('hlsError', {
      fatal: true,
      type: 'networkError',
      details: 'manifestLoadError',
      response: { code: 403 },
    });
    await fixture.whenStable();
    expect(fixture.componentInstance.expired).toBe(0);
    expect(fixture.componentInstance.errors.map((e) => e.code)).toEqual(['unauthorized']);
    expect(player.getAttribute('data-state')).toBe('error');
  });

  it('reports an error and does nothing else when src and streamId are both set', async () => {
    const { fixture, player, factory } = await render();
    fixture.componentInstance.streamId.set('hall-a');
    await fixture.whenStable();
    expect(player.getAttribute('data-state')).toBe('error');
    expect(fixture.componentInstance.errors).toHaveLength(1);
    expect(fixture.componentInstance.errors[0].message).toContain('src');
    expect(fixture.componentInstance.errors[0].message).toContain('streamId');
    expect(factory.mock.calls.length).toBeLessThanOrEqual(1);
  });
});
