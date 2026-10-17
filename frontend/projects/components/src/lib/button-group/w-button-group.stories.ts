import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { WButtonGroup, WButtonGroupItem } from './w-button-group';

const meta: Meta<WButtonGroup> = {
  title: 'Components/Button group',
  component: WButtonGroup,
  decorators: [moduleMetadata({ imports: [WButtonGroup, WButtonGroupItem] })],
};

export default meta;
type Story = StoryObj<WButtonGroup>;

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
