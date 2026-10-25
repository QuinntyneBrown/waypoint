import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { Button } from '../button/button';
import { Pagination } from './pagination';

const meta: Meta<Pagination> = {
  title: 'Components/Pagination',
  component: Pagination,
  decorators: [moduleMetadata({ imports: [Pagination, Button] })],
};

export default meta;
type Story = StoryObj<Pagination>;

export const Default: Story = {
  render: () => ({
    template: `
      <nav ph-pagination aria-label="People pages">
        <p>Page 2 of 5 · Showing 26–50 of 112 people</p>
        <div style="display: flex; gap: 0.5rem">
          <button ph-button variant="ghost" type="button">Previous</button>
          <button ph-button variant="ghost" type="button">Next</button>
        </div>
      </nav>`,
  }),
};
