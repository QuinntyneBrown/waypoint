import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { Button } from '../button/button';
import { Icon } from '../icon/icon';
import { IconButton } from '../icon-button/icon-button';
import {
  Dialog,
  DialogBody,
  DialogFooter,
  DialogHeader,
  DialogSubtitle,
  DialogTitle,
} from './dialog';

const meta: Meta<Dialog> = {
  title: 'Components/Dialog',
  component: Dialog,
  args: { size: 'sm' },
  argTypes: { size: { control: 'select', options: ['', 'sm', 'lg'] } },
  decorators: [
    moduleMetadata({
      imports: [
        Dialog,
        DialogHeader,
        DialogTitle,
        DialogSubtitle,
        DialogBody,
        DialogFooter,
        Button,
        IconButton,
        Icon,
      ],
    }),
  ],
  parameters: { layout: 'fullscreen' },
  render: (args) => ({
    props: args,
    template: `
      <div w-dialog [size]="size" aria-labelledby="dlg-title">
        <div w-dialog-header>
          <div>
            <h2 w-dialog-title id="dlg-title">Decline application</h2>
            <p w-dialog-subtitle>Maya Chen</p>
          </div>
          <button w-icon-button type="button" aria-label="Close"><w-icon name="x" /></button>
        </div>
        <div w-dialog-body>The applicant is told softly. You can reopen this later.</div>
        <div w-dialog-footer>
          <button w-button variant="ghost" type="button">Cancel</button>
          <button w-button variant="primary" type="button">Decline</button>
        </div>
      </div>`,
  }),
};

export default meta;
type Story = StoryObj<Dialog>;

export const Small: Story = {};
export const Large: Story = { args: { size: 'lg' } };
