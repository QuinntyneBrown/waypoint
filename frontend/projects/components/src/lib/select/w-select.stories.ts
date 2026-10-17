import type { Meta, StoryObj } from '@storybook/angular';
import { WSelect } from './w-select';

const meta: Meta<WSelect> = {
  title: 'Components/Select',
  component: WSelect,
  render: () => ({
    template: `<select w-select><option>All youth</option><option>One cohort</option></select>`,
  }),
};

export default meta;
type Story = StoryObj<WSelect>;

export const Default: Story = {};
