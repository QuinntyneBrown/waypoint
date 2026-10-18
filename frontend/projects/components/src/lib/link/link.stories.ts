import type { Meta, StoryObj } from '@storybook/angular';
import { Link } from './link';

const meta: Meta<Link> = {
  title: 'Components/Link',
  component: Link,
  args: { standalone: false },
  render: (args) => ({
    props: args,
    template: `<p>Questions? Read the <a w-link href="#" [standalone]="standalone">help centre</a>.</p>`,
  }),
};

export default meta;
type Story = StoryObj<Link>;

export const Inline: Story = {};
export const Standalone: Story = { args: { standalone: true } };
