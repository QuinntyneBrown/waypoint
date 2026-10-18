import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { Skeleton } from './skeleton';

const meta: Meta<Skeleton> = {
  title: 'Components/Skeleton',
  component: Skeleton,
  decorators: [moduleMetadata({ imports: [Skeleton] })],
};

export default meta;
type Story = StoryObj<Skeleton>;

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
