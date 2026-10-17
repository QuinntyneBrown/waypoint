import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { WButton } from '../button/w-button';
import { WPagination } from './w-pagination';

const meta: Meta<WPagination> = {
  title: 'Components/Pagination',
  component: WPagination,
  decorators: [moduleMetadata({ imports: [WPagination, WButton] })],
};

export default meta;
type Story = StoryObj<WPagination>;

export const Default: Story = {
  render: () => ({
    template: `
      <nav w-pagination aria-label="People pages">
        <p>Page 2 of 5 · Showing 26–50 of 112 people</p>
        <div style="display: flex; gap: 0.5rem">
          <button w-button variant="ghost" type="button">Previous</button>
          <button w-button variant="ghost" type="button">Next</button>
        </div>
      </nav>`,
  }),
};
