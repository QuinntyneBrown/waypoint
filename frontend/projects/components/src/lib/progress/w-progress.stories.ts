import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { WProgress, WProgressRing, WProgressRingLabel } from './w-progress';

const meta: Meta<WProgress> = {
  title: 'Components/Progress',
  component: WProgress,
  args: { value: 60, label: 'Lesson progress' },
  decorators: [moduleMetadata({ imports: [WProgress, WProgressRing, WProgressRingLabel] })],
  render: (args) => ({
    props: args,
    template: `<div w-progress [value]="value" [label]="label"></div>`,
  }),
};

export default meta;
type Story = StoryObj<WProgress>;

export const Bar: Story = {};

export const Ring: Story = {
  render: () => ({
    template: `<div w-progress-ring size="lg" [value]="72"><span w-progress-ring-label>72%</span></div>`,
  }),
};
