import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { ButtonGroup } from './button-group';
import { ButtonGroupItem } from './button-group-item';

const meta: Meta<ButtonGroup> = {
  title: 'Components/Button group',
  component: ButtonGroup,
  decorators: [moduleMetadata({ imports: [ButtonGroup, ButtonGroupItem] })],
};

export default meta;
type Story = StoryObj<ButtonGroup>;

export const Default: Story = {
  render: () => ({
    template: `
      <div w-button-group role="group" aria-label="Theme">
        <button w-button-group-item type="button" active>System</button>
        <button w-button-group-item type="button">Light</button>
        <button w-button-group-item type="button">Dark</button>
      </div>`,
  }),
};
