import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { WTab, WTabs } from './w-tabs';

const meta: Meta<WTabs> = {
  title: 'Components/Tabs',
  component: WTabs,
  decorators: [moduleMetadata({ imports: [WTabs, WTab] })],
};

export default meta;
type Story = StoryObj<WTabs>;

export const Buttons: Story = {
  render: () => ({
    template: `
      <div w-tabs role="tablist" aria-label="Report views">
        <button w-tab type="button" role="tab" active aria-selected="true">Overview</button>
        <button w-tab type="button" role="tab" aria-selected="false">Youth report</button>
        <button w-tab type="button" role="tab" aria-selected="false">Cohort report</button>
      </div>`,
  }),
};

export const Links: Story = {
  render: () => ({
    template: `
      <nav w-tabs aria-label="Settings">
        <a w-tab href="#" active>Profile</a>
        <a w-tab href="#">Notifications</a>
        <a w-tab href="#">Privacy</a>
      </nav>`,
  }),
};
