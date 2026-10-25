import type { Meta, StoryObj } from '@storybook/angular';
import { Avatar } from './avatar';

const meta: Meta<Avatar> = {
  title: 'Components/Avatar',
  component: Avatar,
  args: { name: 'Jordan Rivera' },
  argTypes: {
    size: { control: 'select', options: [undefined, 'xs', 'sm', 'lg', 'xl'] },
  },
};

export default meta;
type Story = StoryObj<Avatar>;

export const Default: Story = {};

export const Sizes: Story = {
  render: (args) => ({
    props: args,
    template: `<div style="display: flex; gap: 12px; align-items: center">
      <ph-avatar [name]="name" size="xs" />
      <ph-avatar [name]="name" size="sm" />
      <ph-avatar [name]="name" />
      <ph-avatar [name]="name" size="lg" />
      <ph-avatar [name]="name" size="xl" />
    </div>`,
  }),
};
