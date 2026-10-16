import { expect, test } from '@playwright/test';
import { BadgeBuilderPage } from '../e2e/pages';
import { seedYouthCurriculum } from '../e2e/support/api';
import { shotOf, signInAsPat } from './curriculum-authoring.helpers';

const FOLDER = '045-curriculum-authoring-author-badges';

test('capture: author badges', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  await signInAsPat(page, request);
  const badges = new BadgeBuilderPage(page);
  await badges.goto();
  await expect(badges.heading).toBeVisible();
  const form = page.locator('form.admin-form');

  const autoName = 'Foundations finisher ' + crypto.randomUUID().slice(0, 6);
  await badges.name.fill(autoName);
  await badges.icon.fill('award');
  await page.getByLabel('Badge description').fill('Earned by finishing a whole module.');
  await badges.criteriaKind.selectOption('Auto');
  await badges.autoRule.selectOption('module-completed');
  await badges.target.selectOption({ label: seeded.firstModuleName });
  await badges.competency.selectOption({ label: seeded.mentorCompetencyName });
  await shotOf(form, FOLDER, 'auto-form', 16);

  await badges.saveBadge.click();
  const automatic = badges.badge(autoName);
  await expect(automatic).toContainText('module-completed');
  await shotOf(automatic, FOLDER, 'auto-card', 12);

  const manualName = 'Servant heart ' + crypto.randomUUID().slice(0, 6);
  await badges.name.fill(manualName);
  await badges.icon.fill('star');
  await page.getByLabel('Badge description').fill('Recognition a mentor gives by hand.');
  await badges.criteriaKind.selectOption('Manual');
  await expect(badges.autoRule).toHaveCount(0);
  await shotOf(form, FOLDER, 'manual-form', 16);
  await badges.saveBadge.click();
  const manual = badges.badge(manualName);
  await expect(manual.getByRole('button', { name: 'Award badge' })).toBeVisible();
  await shotOf(manual, FOLDER, 'manual-card', 12);

  await automatic.getByRole('button', { name: 'Archive badge' }).click();
  await expect(automatic).toHaveCount(0);
  await expect(badges.outcome).toContainText('archived');
  await shotOf(badges.outcome, FOLDER, 'archived', 12);
});
