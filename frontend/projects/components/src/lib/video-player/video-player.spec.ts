import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { NEVER } from 'rxjs';
import { VideoPlayerError } from './types';
import { VIDEO_STREAM_TRANSPORT, VideoStreamTransport } from './video-stream-transport';
import { VideoPlayer } from './video-player';

@Component({
  imports: [VideoPlayer],
  template: `<w-video-player streamId="hall-a" (error)="errors.push($event)" />`,
})
class Host {
  errors: VideoPlayerError[] = [];
}

function transport(describe: VideoStreamTransport['describe']): VideoStreamTransport {
  return { describe, subscribe: () => NEVER, connectionEvents: NEVER };
}

async function render(describe: VideoStreamTransport['describe']) {
  TestBed.configureTestingModule({
    imports: [Host],
    providers: [{ provide: VIDEO_STREAM_TRANSPORT, useValue: transport(describe) }],
  });
  const fixture = TestBed.createComponent(Host);
  await fixture.whenStable();
  const player = fixture.nativeElement.querySelector('w-video-player') as HTMLElement;
  return { fixture, player };
}

describe('VideoPlayer', () => {
  beforeEach(() => {
    vi.stubGlobal(
      'ResizeObserver',
      class {
        observe() {}
        disconnect() {}
      },
    );
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('reports connecting while the stream is described', async () => {
    const { player } = await render(() => new Promise(() => undefined));
    expect(player.getAttribute('data-state')).toBe('connecting');
  });

  it('shows an alert with Retry and emits unauthorized when access is refused', async () => {
    const { fixture, player } = await render(() =>
      Promise.reject(Object.assign(new Error('denied'), { statusCode: 401 })),
    );
    await fixture.whenStable();
    expect(player.getAttribute('data-state')).toBe('error');
    expect(player.querySelector('[role="alert"]')).not.toBeNull();
    expect(player.querySelector('.w-video-player__retry')).not.toBeNull();
    expect(player.querySelector('[data-control="live"]')).toBeNull();
    expect(fixture.componentInstance.errors.map((e) => e.code)).toEqual(['unauthorized']);
  });
});
