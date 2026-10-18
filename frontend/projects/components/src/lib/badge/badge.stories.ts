import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { Badge } from './badge';

const meta: Meta<Badge> = {
  title: 'Components/Badge',
  component: Badge,
  args: { variant: 'success', dot: false },
  argTypes: {
    variant: {
      control: 'select',
      options: ['', 'success', 'warning', 'error', 'danger', 'info', 'lime', 'ink', 'outline'],
    },
  },
  decorators: [moduleMetadata({ imports: [Badge] })],
  render: (args) => ({
    props: args,
    template: `<span w-badge [variant]="variant" [dot]="dot">Approved</span>`,
  }),
};

export default meta;
type Story = StoryObj<Badge>;

export const Success: Story = {};
export const Warning: Story = { args: { variant: 'warning' } };
export const Error: Story = { args: { variant: 'error' } };
export const Outline: Story = { args: { variant: 'outline' } };
export const WithDot: Story = { args: { variant: 'info', dot: true } };
