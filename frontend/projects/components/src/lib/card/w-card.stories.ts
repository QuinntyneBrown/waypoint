import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { WCard, WCardBody, WCardFooter, WCardHeader, WCardTitle } from './w-card';

const meta: Meta<WCard> = {
  title: 'Components/Card',
  component: WCard,
  args: { variant: '' },
  argTypes: {
    variant: { control: 'select', options: ['', 'greige', 'ink', 'lime', 'flat'] },
  },
  decorators: [
    moduleMetadata({ imports: [WCard, WCardHeader, WCardTitle, WCardBody, WCardFooter] }),
  ],
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
type Story = StoryObj<WCard>;

export const Default: Story = {};
export const Greige: Story = { args: { variant: 'greige' } };
export const Ink: Story = { args: { variant: 'ink' } };
export const Lime: Story = { args: { variant: 'lime' } };
export const Flat: Story = { args: { variant: 'flat' } };
