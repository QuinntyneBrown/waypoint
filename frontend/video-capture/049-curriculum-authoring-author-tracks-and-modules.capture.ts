import { expect, test } from '@playwright/test';
import { CurriculumBuilderPage } from '../e2e/pages';
import { shotOf, signInAsPat } from './curriculum-authoring.helpers';

const FOLDER = '049-curriculum-authoring-author-tracks-and-modules';

test('capture: author tracks and modules', async ({ page, request }) => {
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
  await shotOf(page.locator('section.w-card').first(), FOLDER, 'track-picker', 8);

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
  await shotOf(builder.module(one).locator('header.item-head'), FOLDER, 'module-draft', 12);

  const three = 'Leading well ' + tag;
  await page.getByLabel('Module name').fill(three);
  await page.getByLabel('Module description').fill('Lead a small team.');
  await page.getByLabel('Scripture anchor').fill('Philippians 2:3');
  await page.getByRole('button', { name: 'Add module' }).click();
  await expect(builder.module(three)).toBeVisible();

  const moduleNames = page.getByTestId('curriculum-module').locator('h2');
  await builder.module(three).getByRole('button', { name: 'Move module up' }).click();
  await expect(moduleNames.nth(1)).toHaveText(three);
  await builder.module(three).getByRole('button', { name: 'Move module up' }).click();
  await expect(moduleNames.first()).toHaveText(three);
  await expect(builder.outcome).toBeVisible();

  await builder.module(two).getByRole('button', { name: 'Move module up' }).click();
  const conflict = page.getByRole('alert');
  await expect(conflict).toContainText('prerequisite');
  await shotOf(conflict, FOLDER, 'prerequisite-blocked', 12);

  await builder.module(one).getByRole('button', { name: 'Publish module' }).click();
  await expect(builder.outcome).toContainText('Module published.');
  await expect(builder.module(one)).toContainText('Published');
  await shotOf(builder.module(one).locator('header.item-head'), FOLDER, 'module-published', 12);

  await page.getByRole('button', { name: 'Publish track' }).click();
  await expect(builder.outcome).toContainText('Track published.');
  await shotOf(page.locator('section.w-card').first(), FOLDER, 'track-published', 8);
});
