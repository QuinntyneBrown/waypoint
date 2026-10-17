import type { Meta, StoryObj } from '@storybook/angular';
import { WLink } from './w-link';

const meta: Meta<WLink> = {
  title: 'Components/Link',
  component: WLink,
  args: { standalone: false },
  render: (args) => ({
    props: args,
    template: `<p>Questions? Read the <a w-link href="#" [standalone]="standalone">help centre</a>.</p>`,
  }),
};

export default meta;
type Story = StoryObj<WLink>;

export const Inline: Story = {};
export const Standalone: Story = { args: { standalone: true } };
