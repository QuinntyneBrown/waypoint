import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { Card } from './card';
import { CardBody } from './card-body';
import { CardFooter } from './card-footer';
import { CardHeader } from './card-header';
import { CardTitle } from './card-title';

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
      <section ph-card [variant]="variant" style="max-width: 24rem">
        <div ph-card-header><h2 ph-card-title>This week</h2></div>
        <div ph-card-body>Two sessions and one assignment are due.</div>
        <div ph-card-footer>Updated today</div>
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
