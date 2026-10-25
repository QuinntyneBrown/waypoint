import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { DescriptionList } from './description-list';

const meta: Meta<DescriptionList> = {
  title: 'Components/Description list',
  component: DescriptionList,
  args: { vertical: false },
  decorators: [moduleMetadata({ imports: [DescriptionList] })],
  render: (args) => ({
    props: args,
    template: `
      <dl ph-description-list [vertical]="vertical">
        <dt>Role</dt><dd>Mentor</dd>
        <dt>Cohort</dt><dd>Spring 2026</dd>
        <dt>Email</dt><dd>ana@example.org</dd>
      </dl>`,
  }),
};

export default meta;
type Story = StoryObj<DescriptionList>;

export const Default: Story = {};
export const Vertical: Story = { args: { vertical: true } };
