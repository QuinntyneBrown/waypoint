import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { WScripture, WScriptureRef } from './w-scripture';

const meta: Meta<WScripture> = {
  title: 'Components/Scripture',
  component: WScripture,
  decorators: [moduleMetadata({ imports: [WScripture, WScriptureRef] })],
};

export default meta;
type Story = StoryObj<WScripture>;

export const WithReference: Story = {
  render: () => ({
    template: `<div w-scripture>“The Son of Man did not come to be served, but to serve.” <span w-scripture-ref>Mark 10:45</span></div>`,
  }),
};

export const Prompt: Story = {
  render: () => ({ template: `<p w-scripture>Where did you serve someone this week?</p>` }),
};
