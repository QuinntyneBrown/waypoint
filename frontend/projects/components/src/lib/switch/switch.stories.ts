import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { Switch } from './switch';
import { SwitchTrack } from './switch-track';

const meta: Meta<Switch> = {
  title: 'Components/Switch',
  component: Switch,
  decorators: [moduleMetadata({ imports: [Switch, SwitchTrack] })],
};

export default meta;
type Story = StoryObj<Switch>;

export const Off: Story = {
  render: () => ({
    template: `<label ph-switch><input type="checkbox" aria-label="Reduce motion" /><span ph-switch-track></span></label>`,
  }),
};

export const On: Story = {
  render: () => ({
    template: `<label ph-switch><input type="checkbox" aria-label="Reduce motion" checked /><span ph-switch-track></span></label>`,
  }),
};
