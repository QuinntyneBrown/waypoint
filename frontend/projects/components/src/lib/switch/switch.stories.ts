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
    template: `<label w-switch><input type="checkbox" aria-label="Reduce motion" /><span w-switch-track></span></label>`,
  }),
};

export const On: Story = {
  render: () => ({
    template: `<label w-switch><input type="checkbox" aria-label="Reduce motion" checked /><span w-switch-track></span></label>`,
  }),
};
