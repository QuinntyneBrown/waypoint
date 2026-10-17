import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { WButton } from '../button/w-button';
import { WIcon } from '../icon/w-icon';
import { WIconButton } from '../icon-button/w-icon-button';
import {
  WDialog,
  WDialogBody,
  WDialogFooter,
  WDialogHeader,
  WDialogSubtitle,
  WDialogTitle,
} from './w-dialog';

const meta: Meta<WDialog> = {
  title: 'Components/Dialog',
  component: WDialog,
  args: { size: 'sm' },
  argTypes: { size: { control: 'select', options: ['', 'sm', 'lg'] } },
  decorators: [
    moduleMetadata({
      imports: [
        WDialog,
        WDialogHeader,
        WDialogTitle,
        WDialogSubtitle,
        WDialogBody,
        WDialogFooter,
        WButton,
        WIconButton,
        WIcon,
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
type Story = StoryObj<WDialog>;

export const Small: Story = {};
export const Large: Story = { args: { size: 'lg' } };
