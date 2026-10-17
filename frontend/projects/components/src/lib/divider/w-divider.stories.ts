import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { WDivider } from './w-divider';

const meta: Meta<WDivider> = {
  title: 'Components/Divider',
  component: WDivider,
  decorators: [moduleMetadata({ imports: [WDivider] })],
  render: () => ({
    template: `<p>Change password</p><div w-divider></div><p>Parent or guardian</p>`,
  }),
};

export default meta;
type Story = StoryObj<WDivider>;

export const Default: Story = {};
