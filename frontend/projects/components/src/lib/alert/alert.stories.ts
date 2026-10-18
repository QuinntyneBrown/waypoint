import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { Alert } from './alert';
import { AlertTitle } from './alert-title';

const meta: Meta<Alert> = {
  title: 'Components/Alert',
  component: Alert,
  args: { variant: 'info', icon: 'info' },
  argTypes: {
    variant: {
      control: 'select',
      options: ['', 'info', 'success', 'warning', 'error', 'danger', 'lime'],
    },
  },
  decorators: [moduleMetadata({ imports: [Alert, AlertTitle] })],
  render: (args) => ({
    props: args,
    template: `<div w-alert [variant]="variant" [icon]="icon">Applications close 30 April.</div>`,
  }),
};

export default meta;
type Story = StoryObj<Alert>;

export const Info: Story = {};
export const Success: Story = { args: { variant: 'success', icon: 'check-circle' } };
export const Error: Story = { args: { variant: 'error', icon: 'alert-circle' } };
export const TextOnly: Story = { args: { variant: 'warning', icon: '' } };
export const WithTitle: Story = {
  args: { variant: 'warning', icon: 'alert-triangle' },
  render: (args) => ({
    props: args,
    template: `
      <div w-alert [variant]="variant" [icon]="icon" role="note">
        <div w-alert-title>Guardian consent missing</div>
        Maya cannot join sessions until Ana signs the form.
      </div>`,
  }),
};
