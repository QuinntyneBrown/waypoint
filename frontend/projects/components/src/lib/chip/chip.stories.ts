import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { Chip } from './chip';
import { ChipRow } from './chip-row';

const meta: Meta<Chip> = {
  title: 'Components/Chip',
  component: Chip,
  decorators: [moduleMetadata({ imports: [Chip, ChipRow] })],
};

export default meta;
type Story = StoryObj<Chip>;

export const Filters: Story = {
  render: () => ({
    template: `
      <div ph-chip-row aria-label="Roster status filter">
        <button ph-chip type="button" active>All</button>
        <button ph-chip type="button">On track</button>
        <button ph-chip type="button">At risk</button>
        <button ph-chip type="button">New</button>
      </div>`,
  }),
};
