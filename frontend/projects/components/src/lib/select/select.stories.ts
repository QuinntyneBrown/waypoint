import type { Meta, StoryObj } from '@storybook/angular';
import { Select } from './select';

const meta: Meta<Select> = {
  title: 'Components/Select',
  component: Select,
  render: () => ({
    template: `<select w-select><option>All youth</option><option>One cohort</option></select>`,
  }),
};

export default meta;
type Story = StoryObj<Select>;

export const Default: Story = {};
