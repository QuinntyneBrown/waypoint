import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { Icon } from '../icon/icon';
import { Checkbox, CheckboxBox } from './checkbox';

const meta: Meta<Checkbox> = {
  title: 'Components/Checkbox',
  component: Checkbox,
  decorators: [moduleMetadata({ imports: [Checkbox, CheckboxBox, Icon] })],
};

export default meta;
type Story = StoryObj<Checkbox>;

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
