import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { ListItem } from './list-item';
import { ListItemBody } from './list-item-body';
import { ListItemMeta } from './list-item-meta';
import { ListItemTitle } from './list-item-title';

const meta: Meta<ListItem> = {
  title: 'Components/List item',
  component: ListItem,
  decorators: [moduleMetadata({ imports: [ListItem, ListItemBody, ListItemTitle, ListItemMeta] })],
};

export default meta;
type Story = StoryObj<ListItem>;

export const Rows: Story = {
  render: () => ({
    template: `
      <div w-list-item>
        <div w-list-item-body>
          <div w-list-item-title>Serving others</div>
          <div w-list-item-meta>In progress · 40%</div>
        </div>
      </div>
      <div w-list-item>
        <div w-list-item-body>
          <div w-list-item-title>Leading a team</div>
          <div w-list-item-meta>Not started · 0%</div>
        </div>
      </div>`,
  }),
};
