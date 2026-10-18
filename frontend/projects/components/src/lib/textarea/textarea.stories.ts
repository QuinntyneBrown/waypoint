import type { Meta, StoryObj } from '@storybook/angular';
import { Textarea } from './textarea';

const meta: Meta<Textarea> = {
  title: 'Components/Textarea',
  component: Textarea,
  render: () => ({
    template: `<textarea w-textarea rows="4" placeholder="Share how your week is going"></textarea>`,
  }),
};

export default meta;
type Story = StoryObj<Textarea>;

export const Default: Story = {};
