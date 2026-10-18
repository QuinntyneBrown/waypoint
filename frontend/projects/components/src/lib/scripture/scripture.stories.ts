import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { Scripture } from './scripture';
import { ScriptureRef } from './scripture-ref';

const meta: Meta<Scripture> = {
  title: 'Components/Scripture',
  component: Scripture,
  decorators: [moduleMetadata({ imports: [Scripture, ScriptureRef] })],
};

export default meta;
type Story = StoryObj<Scripture>;

export const WithReference: Story = {
  render: () => ({
    template: `<div w-scripture>“The Son of Man did not come to be served, but to serve.” <span w-scripture-ref>Mark 10:45</span></div>`,
  }),
};

export const Prompt: Story = {
  render: () => ({ template: `<p w-scripture>Where did you serve someone this week?</p>` }),
};
