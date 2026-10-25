import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { Matrix } from './matrix';
import { Table } from './table';
import { TableWrap } from './table-wrap';

const meta: Meta<Table> = {
  title: 'Components/Table',
  component: Table,
  args: { cards: false },
  decorators: [moduleMetadata({ imports: [TableWrap, Table, Matrix] })],
  render: (args) => ({
    props: args,
    template: `
      <div ph-table-wrap>
        <table ph-table [cards]="cards" aria-label="People">
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
type Story = StoryObj<Table>;

export const Default: Story = {};
export const Cards: Story = { args: { cards: true } };

export const MatrixTable: Story = {
  render: () => ({
    template: `
      <div ph-table-wrap>
        <table ph-matrix aria-label="Progress board">
          <thead><tr><th>Youth</th><th>Lesson 1</th><th>Lesson 2</th></tr></thead>
          <tbody><tr><td>Maya Chen</td><td>Done</td><td>In progress</td></tr></tbody>
        </table>
      </div>`,
  }),
};
