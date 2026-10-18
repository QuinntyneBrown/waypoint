import type { Meta, StoryObj } from '@storybook/angular';
import { TextField } from './text-field';

const meta: Meta<TextField> = {
  title: 'Components/Text field',
  component: TextField,
  render: () => ({
    template: `<input w-text-field type="text" placeholder="name@school.org" />`,
  }),
};

export default meta;
type Story = StoryObj<TextField>;

export const Default: Story = {};
