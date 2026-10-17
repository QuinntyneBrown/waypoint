import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { WQuizOption } from './w-quiz-option';

const meta: Meta<WQuizOption> = {
  title: 'Components/Quiz option',
  component: WQuizOption,
  decorators: [moduleMetadata({ imports: [WQuizOption] })],
};

export default meta;
type Story = StoryObj<WQuizOption>;

export const Attempt: Story = {
  render: () => ({
    template: `
      <label w-quiz-option><input type="radio" name="q" /><span>Serve others first</span></label>
      <label w-quiz-option selected><input type="radio" name="q" checked /><span>Lead from the front</span></label>`,
  }),
};

export const Review: Story = {
  render: () => ({
    template: `
      <ul style="list-style: none; padding: 0">
        <li w-quiz-option selected>Lead from the front <strong>Youth answer</strong></li>
        <li w-quiz-option correct>Serve others first <strong>Correct answer</strong></li>
      </ul>`,
  }),
};
