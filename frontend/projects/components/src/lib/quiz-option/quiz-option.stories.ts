import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { QuizOption } from './quiz-option';

const meta: Meta<QuizOption> = {
  title: 'Components/Quiz option',
  component: QuizOption,
  decorators: [moduleMetadata({ imports: [QuizOption] })],
};

export default meta;
type Story = StoryObj<QuizOption>;

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
