import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { WListItem, WListItemBody, WListItemMeta, WListItemTitle } from './w-list-item';

const meta: Meta<WListItem> = {
  title: 'Components/List item',
  component: WListItem,
  decorators: [
    moduleMetadata({ imports: [WListItem, WListItemBody, WListItemTitle, WListItemMeta] }),
  ],
};

export default meta;
type Story = StoryObj<WListItem>;

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
