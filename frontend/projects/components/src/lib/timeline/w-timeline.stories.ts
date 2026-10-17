import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { WTimeline, WTimelineItem } from './w-timeline';

const meta: Meta<WTimeline> = {
  title: 'Components/Timeline',
  component: WTimeline,
  decorators: [moduleMetadata({ imports: [WTimeline, WTimelineItem] })],
};

export default meta;
type Story = StoryObj<WTimeline>;

export const Journey: Story = {
  render: () => ({
    template: `
      <div w-timeline>
        <article w-timeline-item done><strong>Welcome</strong><p>Complete</p></article>
        <article w-timeline-item current><strong>Serving others</strong><p>3 of 6 items</p></article>
        <article w-timeline-item><strong>Leading a team</strong><p>Locked</p></article>
      </div>`,
  }),
};
