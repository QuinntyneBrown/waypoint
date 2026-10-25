import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { Icon } from '../icon/icon';
import { Checkbox } from './checkbox';
import { CheckboxBox } from './checkbox-box';

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
      <label ph-checkbox>
        <input type="checkbox" />
        <span ph-checkbox-box><ph-icon name="check" /></span>
        <span>Keep me signed in</span>
      </label>`,
  }),
};

export const Checked: Story = {
  render: () => ({
    template: `
      <label ph-checkbox>
        <input type="checkbox" checked />
        <span ph-checkbox-box><ph-icon name="check" /></span>
        <span>Keep me signed in</span>
      </label>`,
  }),
};

export const Disabled: Story = {
  render: () => ({
    template: `
      <label ph-checkbox>
        <input type="checkbox" disabled />
        <span ph-checkbox-box><ph-icon name="check" /></span>
        <span>Keep me signed in</span>
      </label>`,
  }),
};
