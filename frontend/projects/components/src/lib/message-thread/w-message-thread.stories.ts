import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { WMessageBubble, WMessageBubbleTime, WMessageThread } from './w-message-thread';

const meta: Meta<WMessageThread> = {
  title: 'Components/Message thread',
  component: WMessageThread,
  decorators: [moduleMetadata({ imports: [WMessageThread, WMessageBubble, WMessageBubbleTime] })],
};

export default meta;
type Story = StoryObj<WMessageThread>;

export const Conversation: Story = {
  render: () => ({
    template: `
      <div w-message-thread role="log" aria-live="polite" style="max-width: 28rem">
        <article w-message-bubble direction="in">
          <span class="w-sr-only">Maya:</span>
          <span>Can we move Thursday's check-in?</span>
          <time w-message-bubble-time datetime="2026-10-15T14:00:00Z">Oct 15, 2:00 PM</time>
        </article>
        <article w-message-bubble direction="out">
          <span class="w-sr-only">You:</span>
          <span>Yes. How about Friday?</span>
          <time w-message-bubble-time datetime="2026-10-15T14:05:00Z">Oct 15, 2:05 PM</time>
        </article>
      </div>`,
  }),
};
