import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { Radio } from './radio';
import { RadioBox } from './radio-box';

const meta: Meta<Radio> = {
  title: 'Components/Radio',
  component: Radio,
  decorators: [moduleMetadata({ imports: [Radio, RadioBox] })],
};

export default meta;
type Story = StoryObj<Radio>;

export const Group: Story = {
  render: () => ({
    template: `
      <fieldset>
        <legend>Your RSVP</legend>
        <label ph-radio><input type="radio" name="rsvp" value="going" checked /><span ph-radio-box></span><span>Going</span></label>
        <label ph-radio><input type="radio" name="rsvp" value="maybe" /><span ph-radio-box></span><span>Maybe</span></label>
        <label ph-radio><input type="radio" name="rsvp" value="no" /><span ph-radio-box></span><span>Can’t make it</span></label>
      </fieldset>`,
  }),
};
