import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { Progress, ProgressRing, ProgressRingLabel } from './progress';

const meta: Meta<Progress> = {
  title: 'Components/Progress',
  component: Progress,
  args: { value: 60, label: 'Lesson progress' },
  decorators: [moduleMetadata({ imports: [Progress, ProgressRing, ProgressRingLabel] })],
  render: (args) => ({
    props: args,
    template: `<div w-progress [value]="value" [label]="label"></div>`,
  }),
};

export default meta;
type Story = StoryObj<Progress>;

export const Bar: Story = {};

export const Ring: Story = {
  render: () => ({
    template: `<div w-progress-ring size="lg" [value]="72"><span w-progress-ring-label>72%</span></div>`,
  }),
};
