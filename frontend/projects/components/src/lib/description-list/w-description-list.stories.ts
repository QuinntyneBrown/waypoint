import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { WDescriptionList } from './w-description-list';

const meta: Meta<WDescriptionList> = {
  title: 'Components/Description list',
  component: WDescriptionList,
  args: { vertical: false },
  decorators: [moduleMetadata({ imports: [WDescriptionList] })],
  render: (args) => ({
    props: args,
    template: `
      <dl w-description-list [vertical]="vertical">
        <dt>Role</dt><dd>Mentor</dd>
        <dt>Cohort</dt><dd>Spring 2026</dd>
        <dt>Email</dt><dd>ana@example.org</dd>
      </dl>`,
  }),
};

export default meta;
type Story = StoryObj<WDescriptionList>;

export const Default: Story = {};
export const Vertical: Story = { args: { vertical: true } };
