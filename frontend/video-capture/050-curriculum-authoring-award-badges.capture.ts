import { expect, test } from '@playwright/test';
import { BadgeBuilderPage } from '../e2e/pages';
import { shotOf, signInAsPat } from './curriculum-authoring.helpers';

const FOLDER = '050-curriculum-authoring-award-badges';

test('capture: award badges', async ({ page, request }) => {
  const res = await request.post('/api/_test/users', {
    data: {
      email: `amara-${crypto.randomUUID()}@waypoint.demo`,
      password: 'Password123',
      displayName: 'Amara Bell',
      role: 'Youth',
    },
  });
  expect(res.ok()).toBeTruthy();
  const { userId } = (await res.json()) as { userId: string };

  await signInAsPat(page, request);
  const badges = new BadgeBuilderPage(page);
  await badges.goto();
  await expect(badges.heading).toBeVisible();

  const name = 'Servant heart ' + crypto.randomUUID().slice(0, 6);
  await badges.name.fill(name);
  await badges.icon.fill('star');
  await page.getByLabel('Badge description').fill('Recognition a mentor gives by hand.');
  await badges.criteriaKind.selectOption('Manual');
  await badges.saveBadge.click();
  const manual = badges.badge(name);
  await expect(manual).toContainText('0');
  await shotOf(manual, FOLDER, 'manual-card', 12);

  await manual.getByRole('button', { name: 'Award badge' }).click();
  const dialog = page.getByRole('dialog', { name: 'Award ' + name });
  await expect(dialog).toBeVisible();
  await badges.youth.selectOption(userId);
  await shotOf(dialog, FOLDER, 'dialog');

  await page.getByRole('button', { name: 'Confirm award' }).click();
  await expect(badges.outcome).toContainText('awarded to Amara Bell');
  await shotOf(badges.outcome, FOLDER, 'awarded', 12);
  await expect(manual).toContainText('1');
  await shotOf(manual, FOLDER, 'count', 12);
});
