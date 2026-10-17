import type { Meta, StoryObj } from '@storybook/angular';
import { WTextField } from './w-text-field';

const meta: Meta<WTextField> = {
  title: 'Components/Text field',
  component: WTextField,
  render: () => ({
    template: `<input w-text-field type="text" placeholder="name@school.org" />`,
  }),
};

export default meta;
type Story = StoryObj<WTextField>;

export const Default: Story = {};
