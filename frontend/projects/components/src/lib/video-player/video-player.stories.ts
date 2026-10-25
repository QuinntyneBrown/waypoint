import type { Meta, StoryObj } from '@storybook/angular';
import { applicationConfig } from '@storybook/angular';
import { NEVER } from 'rxjs';
import { DemoTransport } from './demo/demo-transport';
import { VIDEO_PLAYER_I18N } from './i18n';
import { VIDEO_STREAM_TRANSPORT } from './video-stream-transport';
import { VideoPlayer } from './video-player';

/** Provides a demo transport and exposes it as `window.wDemo` so a script can drive the stream. */
const transport = (options?: ConstructorParameters<typeof DemoTransport>[0]) => ({
  provide: VIDEO_STREAM_TRANSPORT,
  useFactory: () => {
    const demo = new DemoTransport(options);
    (window as unknown as { wDemo: DemoTransport }).wDemo = demo;
    return demo;
  },
});

const meta: Meta<VideoPlayer> = {
  title: 'Components/Video Player',
  component: VideoPlayer,
  decorators: [applicationConfig({ providers: [transport()] })],
  args: { streamId: 'lecture-hall-a', autoplay: true, muted: false, volume: 100, captions: null },
  render: (args) => ({
    props: args,
    template: `<div style="max-width: 800px"><ph-video-player [streamId]="streamId" [captions]="captions" [autoplay]="autoplay" [muted]="muted" [volume]="volume" /></div>`,
  }),
};

export default meta;
type Story = StoryObj<VideoPlayer>;

export const Idle: Story = { args: { streamId: null } };

export const Live: Story = { args: { muted: true } };

export const Connecting: Story = {
  decorators: [
    applicationConfig({
      providers: [
        {
          provide: VIDEO_STREAM_TRANSPORT,
          useValue: {
            describe: () => new Promise(() => undefined),
            subscribe: () => NEVER,
            connectionEvents: NEVER,
          },
        },
      ],
    }),
  ],
};

export const WithCaptions: Story = {
  args: { muted: true, captions: { src: 'demo/demo.vtt', srclang: 'en', label: 'English' } },
};

export const Localised: Story = {
  args: { muted: true },
  decorators: [
    applicationConfig({
      providers: [
        transport({ title: 'Salle A' }),
        {
          provide: VIDEO_PLAYER_I18N,
          useValue: {
            play: 'Lecture',
            pause: 'Pause',
            mute: 'Couper le son',
            unmute: 'Activer le son',
            fullscreen: 'Plein écran',
            exitFullscreen: 'Quitter le plein écran',
            liveBadge: 'EN DIRECT',
          },
        },
      ],
    }),
  ],
};

export const Themed: Story = {
  args: { muted: true },
  render: (args) => ({
    props: args,
    template: `<div style="max-width: 800px; --ph-video-player-accent: #0b6b57; --ph-video-player-accent-fg: #fff; --ph-video-player-radius: 0"><ph-video-player [streamId]="streamId" [muted]="muted" /></div>`,
  }),
};

export const Unauthorized: Story = {
  decorators: [
    applicationConfig({
      providers: [
        transport({ describeError: Object.assign(new Error('denied'), { statusCode: 401 }) }),
      ],
    }),
  ],
};
