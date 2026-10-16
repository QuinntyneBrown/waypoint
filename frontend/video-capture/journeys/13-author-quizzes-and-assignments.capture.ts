import { expect, test } from '@playwright/test';
import { CurriculumBuilderPage } from '../../e2e/pages';
import { seedYouthCurriculum } from '../../e2e/support/api';
import { shotOf, signInAsPat } from '../curriculum-authoring.helpers';

const FOLDER = 'journeys/13-author-quizzes-and-assignments';

test('capture: author quizzes and assignments', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  await signInAsPat(page, request);
  const builder = new CurriculumBuilderPage(page);
  await builder.goto();
  await builder.trackSelect.selectOption(seeded.primaryTrackId);
  const module = builder.module(seeded.firstModuleName);
  await expect(module).toBeVisible();

  await module.getByRole('button', { name: 'Add quiz' }).click();
  const quiz = builder.quizDialog;
  await quiz.getByLabel('Pass mark').fill('80');
  await quiz.getByLabel('Attempts allowed').fill('3');
  await quiz.getByLabel('Feedback mode').selectOption('after-all-attempts');
  await quiz.getByRole('checkbox', { name: 'Shuffle questions' }).check();
  await quiz.getByRole('checkbox', { name: 'Required for module completion' }).check();
  await shotOf(quiz, FOLDER, 'quiz-rules');
  await quiz.getByRole('button', { name: 'Create quiz' }).click();

  const editor = builder.questionEditor;
  await expect(editor).toBeVisible();
  await expect(builder.outcome).toContainText('Quiz created');
  const form = editor.locator('form.question-form');
  await editor.getByLabel('Question text').fill('What did Jesus say he came to do?');
  await editor.getByLabel('Question type').selectOption('MultipleChoice');
  await editor.getByLabel('Answer options').fill('To be served\nTo serve\nTo be admired');
  await editor.getByLabel('Correct option numbers').fill('2');
  await shotOf(form, FOLDER, 'question', 16);
  await editor.getByRole('button', { name: 'Add question' }).click();
  await expect(editor).toContainText('What did Jesus say he came to do?');

  await module.scrollIntoViewIfNeeded();
  await module.getByRole('button', { name: 'Add assignment' }).click();
  const dialog = builder.assignmentDialog;
  await dialog.getByLabel('Assignment title').fill('Serving others');
  await dialog
    .getByLabel('Task description')
    .fill('Describe one concrete way you served someone this week.');
  await dialog.getByRole('checkbox', { name: 'Text response' }).check();
  await dialog.getByRole('checkbox', { name: 'Link' }).check();
  await dialog.getByLabel('Rubric criterion').fill('Specific and reflective');
  await dialog.getByLabel('Criterion points').fill('5');
  await dialog.getByRole('button', { name: 'Add rubric criterion' }).click();
  await dialog.getByLabel('Rubric criterion').nth(1).fill('Connects to Scripture');
  await dialog.getByLabel('Criterion points').nth(1).fill('3');
  await dialog.getByLabel('Due date').fill('2026-11-20');
  await dialog.getByLabel('Grace period days').fill('2');
  await dialog.getByRole('checkbox', { name: 'Allow late submissions' }).check();
  await dialog.getByRole('checkbox', { name: 'Require mentor approval' }).check();
  await shotOf(dialog, FOLDER, 'assignment-dialog');

  await dialog.getByRole('button', { name: 'Create assignment' }).click();
  const assignment = builder.assignment('Serving others').first();
  await expect(assignment).toContainText('8 points');
  await shotOf(assignment, FOLDER, 'assignment-draft');

  await assignment.getByRole('button', { name: 'Publish assignment' }).click();
  await expect(builder.outcome).toContainText('Assignment published and assigned');
  await page.evaluate(() => window.scrollTo(0, 0));
  await shotOf(builder.outcome, FOLDER, 'published', 12);
});
