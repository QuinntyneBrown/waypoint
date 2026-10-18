import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { Card, CardBody, CardFooter, CardHeader, CardTitle } from './card';

const meta: Meta<Card> = {
  title: 'Components/Card',
  component: Card,
  args: { variant: '' },
  argTypes: {
    variant: { control: 'select', options: ['', 'greige', 'ink', 'lime', 'flat'] },
  },
  decorators: [moduleMetadata({ imports: [Card, CardHeader, CardTitle, CardBody, CardFooter] })],
  render: (args) => ({
    props: args,
    template: `
      <section w-card [variant]="variant" style="max-width: 24rem">
        <div w-card-header><h2 w-card-title>This week</h2></div>
        <div w-card-body>Two sessions and one assignment are due.</div>
        <div w-card-footer>Updated today</div>
      </section>`,
  }),
};

export default meta;
type Story = StoryObj<Card>;

export const Default: Story = {};
export const Greige: Story = { args: { variant: 'greige' } };
export const Ink: Story = { args: { variant: 'ink' } };
export const Lime: Story = { args: { variant: 'lime' } };
export const Flat: Story = { args: { variant: 'flat' } };
