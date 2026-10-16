import { expect, test } from '@playwright/test';
import { CurriculumBuilderPage } from '../e2e/pages';
import { seedYouthCurriculum } from '../e2e/support/api';
import { shotOf, signInAsPat } from './curriculum-authoring.helpers';

const FOLDER = '048-curriculum-authoring-author-quizzes';

test('capture: author quizzes', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  await signInAsPat(page, request);
  const builder = new CurriculumBuilderPage(page);
  await builder.goto();
  await builder.trackSelect.selectOption(seeded.primaryTrackId);
  const module = builder.module(seeded.firstModuleName);
  await expect(module).toBeVisible();

  await module.getByRole('button', { name: 'Add quiz' }).click();
  const dialog = builder.quizDialog;
  await dialog.getByLabel('Pass mark').fill('80');
  await dialog.getByLabel('Attempts allowed').fill('3');
  await dialog.getByLabel('Feedback mode').selectOption('after-all-attempts');
  await dialog.getByRole('checkbox', { name: 'Shuffle questions' }).check();
  await dialog.getByRole('checkbox', { name: 'Required for module completion' }).check();
  await shotOf(dialog, FOLDER, 'settings');
  await dialog.getByRole('button', { name: 'Create quiz' }).click();

  const editor = builder.questionEditor;
  await expect(editor).toBeVisible();
  await expect(builder.outcome).toContainText('Quiz created');
  const form = editor.locator('form.question-form');

  await editor.getByLabel('Question text').fill('What did Jesus say he came to do?');
  await editor.getByLabel('Question type').selectOption('MultipleChoice');
  await editor.getByLabel('Answer options').fill('To be served\nTo serve\nTo be admired');
  await editor.getByLabel('Correct option numbers').fill('2');
  await shotOf(form, FOLDER, 'choice-question', 16);
  await editor.getByRole('button', { name: 'Add question' }).click();
  await expect(editor).toContainText('What did Jesus say he came to do?');

  await editor.getByLabel('Question text').fill('Name one way you can serve this week.');
  await editor.getByLabel('Question type').selectOption('ShortAnswer');
  await editor.getByLabel('Correct text answers').fill('helping at home\nhelping a neighbour');
  await shotOf(form, FOLDER, 'short-answer', 16);
  await editor.getByRole('button', { name: 'Add question' }).click();
  await expect(editor).toContainText('Name one way you can serve this week.');

  await editor.getByRole('button', { name: 'Move question up' }).last().click();
  await expect(editor.locator('ol.question-list li').first()).toContainText('Name one way');
  await shotOf(editor.locator('form.settings-grid'), FOLDER, 'rules', 12);
});
