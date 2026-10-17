import type { Meta, StoryObj } from '@storybook/angular';
import { WButton } from './w-button';

const meta: Meta<WButton> = {
  title: 'Components/Button',
  component: WButton,
  args: { variant: 'primary', size: 'md', block: false, busy: false },
  argTypes: {
    variant: {
      control: 'select',
      options: ['primary', 'secondary', 'ghost', 'subtle', 'neutral', 'danger', 'link'],
    },
    size: { control: 'select', options: ['sm', 'md', 'lg'] },
  },
  render: (args) => ({
    props: args,
    template: `<button w-button type="button" [variant]="variant" [size]="size" [block]="block" [busy]="busy">Save changes</button>`,
  }),
};

export default meta;
type Story = StoryObj<WButton>;

export const Primary: Story = {};

export const Variants: Story = {
  render: () => ({
    template: `<div style="display: flex; gap: 12px; flex-wrap: wrap">
      <button w-button type="button" variant="primary">Primary</button>
      <button w-button type="button" variant="secondary">Secondary</button>
      <button w-button type="button" variant="ghost">Ghost</button>
      <button w-button type="button" variant="subtle">Subtle</button>
      <button w-button type="button">Neutral</button>
      <button w-button type="button" variant="danger">Danger</button>
      <button w-button type="button" variant="link">Link</button>
    </div>`,
  }),
};

export const Busy: Story = { args: { busy: true } };
