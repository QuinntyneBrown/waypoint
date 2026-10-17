import type { Meta, StoryObj } from '@storybook/angular';
import { applicationConfig } from '@storybook/angular';
import { NEVER } from 'rxjs';
import { VIDEO_STREAM_TRANSPORT, VideoStreamTransport } from './video-stream-transport';
import { WVideoPlayer } from './w-video-player';

const stalled: VideoStreamTransport = {
  describe: () => new Promise(() => undefined),
  subscribe: () => NEVER,
  connectionEvents: NEVER,
};

const refused: VideoStreamTransport = {
  ...stalled,
  describe: () => Promise.reject(Object.assign(new Error('denied'), { statusCode: 401 })),
};

const meta: Meta<WVideoPlayer> = {
  title: 'Components/Video Player',
  component: WVideoPlayer,
  decorators: [
    applicationConfig({ providers: [{ provide: VIDEO_STREAM_TRANSPORT, useValue: stalled }] }),
  ],
  render: (args) => ({
    props: args,
    template: `<div style="max-width: 640px"><w-video-player [streamId]="streamId" /></div>`,
  }),
};

export default meta;
type Story = StoryObj<WVideoPlayer>;

export const Idle: Story = { args: { streamId: null } };

export const Connecting: Story = { args: { streamId: 'lecture-hall-a' } };

export const Unauthorized: Story = {
  args: { streamId: 'lecture-hall-a' },
  decorators: [
    applicationConfig({ providers: [{ provide: VIDEO_STREAM_TRANSPORT, useValue: refused }] }),
  ],
};
