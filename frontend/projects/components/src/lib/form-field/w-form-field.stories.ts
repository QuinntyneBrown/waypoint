import type { Meta, StoryObj } from '@storybook/angular';
import { WErrorText, WFormField, WHelp, WLabel, WLabelRequired } from './w-form-field';

const meta: Meta<WFormField> = {
  title: 'Components/Form field',
  component: WFormField,
  args: { invalid: false },
  render: (args) => ({
    props: args,
    moduleMetadata: { imports: [WLabel, WLabelRequired, WHelp, WErrorText] },
    template: `<div w-form-field [invalid]="invalid">
      <label w-label for="email">Email <span w-label-required>*</span></label>
      <input id="email" type="email" style="width: 100%" />
      <p w-help>Use your school address.</p>
      @if (invalid) { <p w-error-text>Enter a valid email address.</p> }
    </div>`,
  }),
};

export default meta;
type Story = StoryObj<WFormField>;

export const Default: Story = {};
export const Invalid: Story = { args: { invalid: true } };
