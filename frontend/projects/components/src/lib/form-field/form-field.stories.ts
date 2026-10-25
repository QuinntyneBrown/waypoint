import type { Meta, StoryObj } from '@storybook/angular';
import { ErrorText } from './error-text';
import { FormField } from './form-field';
import { Help } from './help';
import { Label } from './label';
import { LabelRequired } from './label-required';

const meta: Meta<FormField> = {
  title: 'Components/Form field',
  component: FormField,
  args: { invalid: false },
  render: (args) => ({
    props: args,
    moduleMetadata: { imports: [Label, LabelRequired, Help, ErrorText] },
    template: `<div ph-form-field [invalid]="invalid">
      <label ph-label for="email">Email <span ph-label-required>*</span></label>
      <input id="email" type="email" style="width: 100%" />
      <p ph-help>Use your school address.</p>
      @if (invalid) { <p ph-error-text>Enter a valid email address.</p> }
    </div>`,
  }),
};

export default meta;
type Story = StoryObj<FormField>;

export const Default: Story = {};
export const Invalid: Story = { args: { invalid: true } };
