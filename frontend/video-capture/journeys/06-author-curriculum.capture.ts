import { expect, test } from '@playwright/test';
import { CurriculumBuilderPage } from '../../e2e/pages';
import { shotOf, signInAsPat } from '../curriculum-authoring.helpers';

const FOLDER = 'journeys/06-author-curriculum';

test('capture: author curriculum', async ({ page, request }) => {
  await signInAsPat(page, request);
  const builder = new CurriculumBuilderPage(page);
  await builder.goto();
  await expect(builder.heading).toBeVisible();

  const tag = crypto.randomUUID().slice(0, 6);
  const trackName = 'Servant Leadership ' + tag;
  await builder.newTrack.click();
  await builder.trackDialog.getByLabel('Track name').fill(trackName);
  await builder.trackDialog.getByLabel('Track key').fill('servant-' + tag);
  await builder.trackDialog
    .getByLabel('Track description')
    .fill('A pathway for learning to lead by serving.');
  await shotOf(builder.trackDialog, FOLDER, 'track-dialog');
  await builder.trackDialog.getByRole('button', { name: 'Create track' }).click();
  await expect(builder.trackSelect.locator('option:checked')).toHaveText(trackName);
  await expect(page.getByText('No modules yet')).toBeVisible();

  const one = 'Foundations ' + tag;
  const two = 'Serving others ' + tag;
  await page.getByLabel('Module name').fill(one);
  await page.getByLabel('Module description').fill('Start with the heart of service.');
  await page.getByLabel('Scripture anchor').fill('Mark 10:45');
  await page.getByRole('button', { name: 'Add module' }).click();
  await expect(builder.module(one)).toBeVisible();

  await page.getByLabel('Module name').fill(two);
  await page.getByLabel('Module description').fill('Put service into practice.');
  await page.getByLabel('Scripture anchor').fill('John 13:14');
  await page.getByLabel('Module prerequisite').selectOption({ label: one });
  await shotOf(page.locator('form.module-form'), FOLDER, 'module-form', 12);
  await page.getByRole('button', { name: 'Add module' }).click();
  await expect(builder.module(two)).toBeVisible();

  await builder.module(two).getByRole('button', { name: 'Move module up' }).click();
  const conflict = page.getByRole('alert');
  await expect(conflict).toContainText('prerequisite');
  await shotOf(conflict, FOLDER, 'prerequisite-blocked', 12);

  const title = 'Serving like Jesus';
  await builder.module(one).getByRole('button', { name: 'Add lesson' }).click();
  const dialog = builder.lessonDialog;
  await dialog.getByLabel('Lesson title').fill(title);
  await dialog.getByLabel('Scripture reference').fill('Mark 10:45');
  await dialog.getByLabel('Estimated minutes').fill('25');
  await dialog.getByLabel('Learning objectives').fill('Name one way Jesus served others.');
  await dialog
    .getByLabel('Teaching body')
    .fill('Jesus came to serve, not to be served. Look at how he treated people around him.');
  await dialog.getByLabel('Reflection prompts').fill('Who can you serve this week?');
  await dialog
    .getByLabel('Leader guide notes')
    .fill('Invite safe, everyday examples. Keep the discussion short.');
  await shotOf(dialog, FOLDER, 'lesson-dialog');
  await dialog.getByRole('button', { name: 'Create lesson' }).click();
  const lesson = builder.lesson(title);
  await expect(lesson).toBeVisible();
  await lesson.getByRole('button', { name: 'Publish lesson' }).click();
  await expect(lesson).toContainText('Published');
  await shotOf(lesson, FOLDER, 'lesson-published');

  await builder.module(one).getByRole('button', { name: 'Publish module' }).click();
  await expect(builder.outcome).toContainText('Module published.');
  await page.getByRole('button', { name: 'Publish track' }).click();
  await expect(builder.outcome).toContainText('Track published.');
  await page.evaluate(() => window.scrollTo(0, 0));
  await shotOf(page.locator('section.w-card').first(), FOLDER, 'track-published', 8);
});
