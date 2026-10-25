import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { Tab } from './tab';
import { Tabs } from './tabs';

const meta: Meta<Tabs> = {
  title: 'Components/Tabs',
  component: Tabs,
  decorators: [moduleMetadata({ imports: [Tabs, Tab] })],
};

export default meta;
type Story = StoryObj<Tabs>;

export const Buttons: Story = {
  render: () => ({
    template: `
      <div ph-tabs role="tablist" aria-label="Report views">
        <button ph-tab type="button" role="tab" active aria-selected="true">Overview</button>
        <button ph-tab type="button" role="tab" aria-selected="false">Youth report</button>
        <button ph-tab type="button" role="tab" aria-selected="false">Cohort report</button>
      </div>`,
  }),
};

export const Links: Story = {
  render: () => ({
    template: `
      <nav ph-tabs aria-label="Settings">
        <a ph-tab href="#" active>Profile</a>
        <a ph-tab href="#">Notifications</a>
        <a ph-tab href="#">Privacy</a>
      </nav>`,
  }),
};
