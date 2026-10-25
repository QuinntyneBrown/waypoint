import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { Button } from '../button/button';
import { Icon } from '../icon/icon';
import { IconButton } from '../icon-button/icon-button';
import { Dialog } from './dialog';
import { DialogBody } from './dialog-body';
import { DialogFooter } from './dialog-footer';
import { DialogHeader } from './dialog-header';
import { DialogSubtitle } from './dialog-subtitle';
import { DialogTitle } from './dialog-title';

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
      <div ph-dialog [size]="size" aria-labelledby="dlg-title">
        <div ph-dialog-header>
          <div>
            <h2 ph-dialog-title id="dlg-title">Decline application</h2>
            <p ph-dialog-subtitle>Maya Chen</p>
          </div>
          <button ph-icon-button type="button" aria-label="Close"><ph-icon name="x" /></button>
        </div>
        <div ph-dialog-body>The applicant is told softly. You can reopen this later.</div>
        <div ph-dialog-footer>
          <button ph-button variant="ghost" type="button">Cancel</button>
          <button ph-button variant="primary" type="button">Decline</button>
        </div>
      </div>`,
  }),
};

export default meta;
type Story = StoryObj<Dialog>;

export const Small: Story = {};
export const Large: Story = { args: { size: 'lg' } };
