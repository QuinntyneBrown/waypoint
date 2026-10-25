import type { Meta, StoryObj } from '@storybook/angular';
import { Icon } from './icon';
import { W_ICONS } from './icons';

const meta: Meta<Icon> = {
  title: 'Components/Icon',
  component: Icon,
  args: { name: 'home' },
  argTypes: {
    name: { control: 'select', options: Object.keys(W_ICONS) },
  },
};

export default meta;
type Story = StoryObj<Icon>;

export const Default: Story = {};

export const AllIcons: Story = {
  render: () => ({
    props: { names: Object.keys(W_ICONS) },
    template: `<div style="display: grid; grid-template-columns: repeat(auto-fill, minmax(120px, 1fr)); gap: 16px">
      @for (n of names; track n) {
        <div style="display: flex; gap: 8px; align-items: center">
          <ph-icon [name]="n" /><small>{{ n }}</small>
        </div>
      }
    </div>`,
  }),
};
