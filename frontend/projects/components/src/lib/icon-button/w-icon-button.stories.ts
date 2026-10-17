import type { Meta, StoryObj } from '@storybook/angular';
import { WIconButton } from './w-icon-button';
import { WIcon } from '../icon/w-icon';

const meta: Meta<WIconButton> = {
  title: 'Components/Icon button',
  component: WIconButton,
  render: () => ({
    moduleMetadata: { imports: [WIcon] },
    template: `<button w-icon-button type="button" aria-label="Close"><w-icon name="x" /></button>`,
  }),
};

export default meta;
type Story = StoryObj<WIconButton>;

export const Default: Story = {};
