import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { WSkeleton } from './w-skeleton';

const meta: Meta<WSkeleton> = {
  title: 'Components/Skeleton',
  component: WSkeleton,
  decorators: [moduleMetadata({ imports: [WSkeleton] })],
};

export default meta;
type Story = StoryObj<WSkeleton>;

export const Loading: Story = {
  render: () => ({
    template: `
      <div aria-busy="true" aria-label="Loading audit activity">
        <span w-skeleton variant="title"></span>
        <span w-skeleton variant="block"></span>
        <span w-skeleton variant="text"></span>
      </div>`,
  }),
};
