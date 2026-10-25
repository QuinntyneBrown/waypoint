import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { Divider } from './divider';

const meta: Meta<Divider> = {
  title: 'Components/Divider',
  component: Divider,
  decorators: [moduleMetadata({ imports: [Divider] })],
  render: () => ({
    template: `<p>Change password</p><div ph-divider></div><p>Parent or guardian</p>`,
  }),
};

export default meta;
type Story = StoryObj<Divider>;

export const Default: Story = {};
