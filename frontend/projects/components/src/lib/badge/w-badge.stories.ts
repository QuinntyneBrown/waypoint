import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { WBadge } from './w-badge';

const meta: Meta<WBadge> = {
  title: 'Components/Badge',
  component: WBadge,
  args: { variant: 'success', dot: false },
  argTypes: {
    variant: {
      control: 'select',
      options: ['', 'success', 'warning', 'error', 'danger', 'info', 'lime', 'ink', 'outline'],
    },
  },
  decorators: [moduleMetadata({ imports: [WBadge] })],
  render: (args) => ({
    props: args,
    template: `<span w-badge [variant]="variant" [dot]="dot">Approved</span>`,
  }),
};

export default meta;
type Story = StoryObj<WBadge>;

export const Success: Story = {};
export const Warning: Story = { args: { variant: 'warning' } };
export const Error: Story = { args: { variant: 'error' } };
export const Outline: Story = { args: { variant: 'outline' } };
export const WithDot: Story = { args: { variant: 'info', dot: true } };
