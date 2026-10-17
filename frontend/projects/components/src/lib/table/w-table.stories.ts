import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { WMatrix, WTable, WTableWrap } from './w-table';

const meta: Meta<WTable> = {
  title: 'Components/Table',
  component: WTable,
  args: { cards: false },
  decorators: [moduleMetadata({ imports: [WTableWrap, WTable, WMatrix] })],
  render: (args) => ({
    props: args,
    template: `
      <div w-table-wrap>
        <table w-table [cards]="cards" aria-label="People">
          <thead><tr><th>Name</th><th>Role</th><th>Status</th></tr></thead>
          <tbody>
            <tr><td data-label="Name">Ana Reyes</td><td data-label="Role">Mentor</td><td data-label="Status">Active</td></tr>
            <tr><td data-label="Name">Maya Chen</td><td data-label="Role">Youth</td><td data-label="Status">Pending</td></tr>
          </tbody>
        </table>
      </div>`,
  }),
};

export default meta;
type Story = StoryObj<WTable>;

export const Default: Story = {};
export const Cards: Story = { args: { cards: true } };

export const Matrix: Story = {
  render: () => ({
    template: `
      <div w-table-wrap>
        <table w-matrix aria-label="Progress board">
          <thead><tr><th>Youth</th><th>Lesson 1</th><th>Lesson 2</th></tr></thead>
          <tbody><tr><td>Maya Chen</td><td>Done</td><td>In progress</td></tr></tbody>
        </table>
      </div>`,
  }),
};
