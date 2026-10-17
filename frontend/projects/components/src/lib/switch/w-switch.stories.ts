import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { WSwitch, WSwitchTrack } from './w-switch';

const meta: Meta<WSwitch> = {
  title: 'Components/Switch',
  component: WSwitch,
  decorators: [moduleMetadata({ imports: [WSwitch, WSwitchTrack] })],
};

export default meta;
type Story = StoryObj<WSwitch>;

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
