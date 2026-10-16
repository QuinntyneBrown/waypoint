import { expect, test } from '@playwright/test';
import { CurriculumBuilderPage } from '../e2e/pages';
import { seedYouthCurriculum } from '../e2e/support/api';
import { shotAround, shotOf, signInAsPat } from './curriculum-authoring.helpers';

const FOLDER = '044-curriculum-authoring-author-assignments';

test('capture: author assignments', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  await signInAsPat(page, request);
  const builder = new CurriculumBuilderPage(page);
  await builder.goto();
  await builder.trackSelect.selectOption(seeded.primaryTrackId);
  const module = builder.module(seeded.firstModuleName);
  await expect(module).toBeVisible();

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
  await shotOf(dialog, FOLDER, 'types-rubric', 16);

  await dialog.getByRole('button', { name: 'Add rubric criterion' }).click();
  await dialog.getByLabel('Rubric criterion').nth(1).fill('Connects to Scripture');
  await dialog.getByLabel('Criterion points').nth(1).fill('3');
  await dialog.getByLabel('Due date').fill('2026-11-20');
  await dialog.getByLabel('Grace period days').fill('2');
  await dialog.getByRole('checkbox', { name: 'Allow late submissions' }).check();
  await dialog.getByRole('checkbox', { name: 'Require mentor approval' }).check();
  await shotOf(dialog.locator('fieldset.rubric-editor'), FOLDER, 'rubric');
  await shotAround(
    [
      dialog.getByLabel('Due date'),
      dialog.getByLabel('Grace period days'),
      dialog.getByRole('checkbox', { name: 'Allow late submissions' }),
      dialog.getByRole('checkbox', { name: 'Require mentor approval' }),
    ],
    FOLDER,
    'due-rules',
    30,
  );

  await dialog.getByRole('button', { name: 'Create assignment' }).click();
  const assignment = builder.assignment('Serving others').first();
  await expect(assignment).toContainText('8 points');
  await shotOf(assignment, FOLDER, 'total');

  await assignment.getByRole('button', { name: 'Publish assignment' }).click();
  await expect(builder.outcome).toContainText('Assignment published and assigned');
  await page.evaluate(() => window.scrollTo(0, 0));
  await shotOf(builder.outcome, FOLDER, 'published', 12);
});
