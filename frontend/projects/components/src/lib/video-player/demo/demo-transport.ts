import { Observable, Subject, Subscriber } from 'rxjs';
import { shiftFragment, splitFragmentedMp4, FragmentedMp4 } from './fmp4-boxes';
import { VideoChunk, VideoStreamDescriptor } from '../types';
import { VideoStreamTransport } from '../video-stream-transport';

/**
 * Story and demo transport: replays a short fragmented MP4 as a never-ending live stream, one
 * fragment per second, and lets a script stall, drop, end or fail the stream.
 */
export class DemoTransport implements VideoStreamTransport {
  readonly connectionEvents = new Subject<'reconnecting' | 'reconnected' | 'closed'>();
  /** While true no fragment is sent and the timeline does not advance. */
  stalled = false;
  private sink?: Subscriber<VideoChunk>;
  private file?: Promise<FragmentedMp4>;

  constructor(
    private readonly options: {
      url?: string;
      title?: string;
      describeError?: unknown;
    } = {},
  ) {}

  async describe(streamId: string): Promise<VideoStreamDescriptor> {
    if (this.options.describeError) throw this.options.describeError;
    return {
      streamId,
      title: this.options.title ?? 'Lecture hall A',
      mimeType: 'video/mp4; codecs="avc1.4d401f,mp4a.40.2"',
      startedAt: new Date().toISOString(),
      width: 640,
      height: 360,
    };
  }

  subscribe(): Observable<VideoChunk> {
    return new Observable<VideoChunk>((subscriber) => {
      this.sink = subscriber;
      this.file ??= fetch(this.options.url ?? 'demo/demo-10s.fmp4')
        .then((response) => response.arrayBuffer())
        .then((buffer) => splitFragmentedMp4(new Uint8Array(buffer)));
      let seq = 0;
      let index = 0;
      const emit = (file: FragmentedMp4) => {
        const loop = Math.floor(index / file.fragments.length);
        const fragment = file.fragments[index % file.fragments.length];
        index++;
        seq++;
        subscriber.next({ kind: 1, seq, data: shiftFragment(fragment, loop, file.periods, seq) });
      };
      let timer: ReturnType<typeof setInterval> | undefined;
      this.file.then(
        (file) => {
          if (subscriber.closed) return;
          subscriber.next({ kind: 0, seq: 0, data: file.init });
          for (let sent = 0; sent < 3; sent++) emit(file);
          timer = setInterval(() => {
            if (!this.stalled) emit(file);
          }, 1000);
        },
        (error) => subscriber.error(error),
      );
      return () => clearInterval(timer);
    });
  }

  /** The source finished: the player shows the ended panel. */
  end(): void {
    this.sink?.complete();
  }

  /** The source failed with a hub error. */
  fail(error: unknown): void {
    this.sink?.error(error);
  }
}
