import type { Meta, StoryObj } from '@storybook/angular';
import { IconButton } from './icon-button';
import { Icon } from '../icon/icon';

const meta: Meta<IconButton> = {
  title: 'Components/Icon button',
  component: IconButton,
  render: () => ({
    moduleMetadata: { imports: [Icon] },
    template: `<button ph-icon-button type="button" aria-label="Close"><ph-icon name="x" /></button>`,
  }),
};

export default meta;
type Story = StoryObj<IconButton>;

export const Default: Story = {};
