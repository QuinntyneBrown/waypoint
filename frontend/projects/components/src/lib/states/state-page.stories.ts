import type { Meta, StoryObj } from '@storybook/angular';
import { StatePage } from './state-page';

const meta: Meta<StatePage> = {
  title: 'Components/State page',
  component: StatePage,
  args: {
    icon: 'alert-triangle',
    tone: 'warning',
    eyebrow: 'Error 404',
    title: 'We couldn’t find that page',
    text: 'The link may be broken or the page may have moved.',
  },
  argTypes: {
    tone: { control: 'select', options: ['warning', 'error', 'info', 'success'] },
  },
  render: (args) => ({
    props: args,
    template: `<w-state-page [icon]="icon" [tone]="tone" [eyebrow]="eyebrow" [title]="title" [text]="text">
      <button class="w-btn w-btn--primary" type="button">Go home</button>
    </w-state-page>`,
  }),
};

export default meta;
type Story = StoryObj<StatePage>;

export const Warning: Story = {};

export const ErrorTone: Story = {
  args: { tone: 'error', icon: 'x-circle', eyebrow: 'Error 500', title: 'Something went wrong' },
};
