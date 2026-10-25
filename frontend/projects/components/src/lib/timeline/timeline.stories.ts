import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { Timeline } from './timeline';
import { TimelineItem } from './timeline-item';

const meta: Meta<Timeline> = {
  title: 'Components/Timeline',
  component: Timeline,
  decorators: [moduleMetadata({ imports: [Timeline, TimelineItem] })],
};

export default meta;
type Story = StoryObj<Timeline>;

export const Journey: Story = {
  render: () => ({
    template: `
      <div ph-timeline>
        <article ph-timeline-item done><strong>Welcome</strong><p>Complete</p></article>
        <article ph-timeline-item current><strong>Serving others</strong><p>3 of 6 items</p></article>
        <article ph-timeline-item><strong>Leading a team</strong><p>Locked</p></article>
      </div>`,
  }),
};
