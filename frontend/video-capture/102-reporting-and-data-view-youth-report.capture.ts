import { expect, test } from '@playwright/test';
import { AdminReportsPage } from '../e2e/pages';
import { seedCast } from './mentoring-cast';
import { seedPat, signInWithRetry } from './reporting-cast';
import { shotFull, shotOf } from './shotOf';

const FOLDER = '102-reporting-and-data-view-youth-report';

test('capture: view youth report', async ({ page, request }) => {
  test.setTimeout(180_000);
  await page.setViewportSize({ width: 1280, height: 800 });
  const seeded = await seedCast(request, 'David Osei');
  const pat = await seedPat(request);
  await signInWithRetry(page, pat);
  const reports = new AdminReportsPage(page);
  await reports.goto();
  await expect(reports.heading).toBeVisible();
  await expect(reports.updatedAt).toBeVisible({ timeout: 30_000 });
  await reports.youthTab.click();
  await expect(reports.generateYouth).toBeVisible({ timeout: 30_000 });
  await page.waitForLoadState('networkidle');
  await reports.youth.selectOption(seeded.youthId);
  await shotOf(page, FOLDER, 'selector', page.locator('.report-selector'), 6);

  await reports.generateYouth.click();
  await expect(reports.youthReport).toContainText('David Osei', { timeout: 30_000 });
  await expect(reports.youthReport).toContainText('50% complete', { timeout: 30_000 });
  await expect(reports.youthReport).toContainText(seeded.journeyBadgeName, { timeout: 30_000 });
  await page.waitForLoadState('networkidle');
  const box = (await reports.youthReport.boundingBox())!;
  const y = box.y + (await page.evaluate(() => window.scrollY));
  await shotFull(page, FOLDER, 'rings', { x: 250, y: y - 10, width: 1030, height: 310 });
  await shotFull(page, FOLDER, 'modules', { x: 250, y: y + 312, width: 1030, height: 260 });
  await shotFull(page, FOLDER, 'notes', { x: 250, y: y + 556, width: 1030, height: 280 });
});
