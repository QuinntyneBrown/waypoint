import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { WIcon } from '../icon/w-icon';
import { WCheckbox, WCheckboxBox } from './w-checkbox';

const meta: Meta<WCheckbox> = {
  title: 'Components/Checkbox',
  component: WCheckbox,
  decorators: [moduleMetadata({ imports: [WCheckbox, WCheckboxBox, WIcon] })],
};

export default meta;
type Story = StoryObj<WCheckbox>;

export const Default: Story = {
  render: () => ({
    template: `
      <label w-checkbox>
        <input type="checkbox" />
        <span w-checkbox-box><w-icon name="check" /></span>
        <span>Keep me signed in</span>
      </label>`,
  }),
};

export const Checked: Story = {
  render: () => ({
    template: `
      <label w-checkbox>
        <input type="checkbox" checked />
        <span w-checkbox-box><w-icon name="check" /></span>
        <span>Keep me signed in</span>
      </label>`,
  }),
};

export const Disabled: Story = {
  render: () => ({
    template: `
      <label w-checkbox>
        <input type="checkbox" disabled />
        <span w-checkbox-box><w-icon name="check" /></span>
        <span>Keep me signed in</span>
      </label>`,
  }),
};
