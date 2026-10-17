import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { WChip, WChipRow } from './w-chip';

const meta: Meta<WChip> = {
  title: 'Components/Chip',
  component: WChip,
  decorators: [moduleMetadata({ imports: [WChip, WChipRow] })],
};

export default meta;
type Story = StoryObj<WChip>;

export const Filters: Story = {
  render: () => ({
    template: `
      <div w-chip-row aria-label="Roster status filter">
        <button w-chip type="button" active>All</button>
        <button w-chip type="button">On track</button>
        <button w-chip type="button">At risk</button>
        <button w-chip type="button">New</button>
      </div>`,
  }),
};
