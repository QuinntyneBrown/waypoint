import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { WRadio, WRadioBox } from './w-radio';

const meta: Meta<WRadio> = {
  title: 'Components/Radio',
  component: WRadio,
  decorators: [moduleMetadata({ imports: [WRadio, WRadioBox] })],
};

export default meta;
type Story = StoryObj<WRadio>;

export const Group: Story = {
  render: () => ({
    template: `
      <fieldset>
        <legend>Your RSVP</legend>
        <label w-radio><input type="radio" name="rsvp" value="going" checked /><span w-radio-box></span><span>Going</span></label>
        <label w-radio><input type="radio" name="rsvp" value="maybe" /><span w-radio-box></span><span>Maybe</span></label>
        <label w-radio><input type="radio" name="rsvp" value="no" /><span w-radio-box></span><span>Can’t make it</span></label>
      </fieldset>`,
  }),
};
