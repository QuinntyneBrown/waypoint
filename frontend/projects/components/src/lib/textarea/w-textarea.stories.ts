import type { Meta, StoryObj } from '@storybook/angular';
import { WTextarea } from './w-textarea';

const meta: Meta<WTextarea> = {
  title: 'Components/Textarea',
  component: WTextarea,
  render: () => ({
    template: `<textarea w-textarea rows="4" placeholder="Share how your week is going"></textarea>`,
  }),
};

export default meta;
type Story = StoryObj<WTextarea>;

export const Default: Story = {};
