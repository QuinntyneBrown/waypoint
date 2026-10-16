import { expect, test } from '@playwright/test';
import { AdminReportsPage } from '../e2e/pages';
import { seedCast } from './mentoring-cast';
import { seedPat, signInWithRetry } from './reporting-cast';
import { shotFull, shotOf } from './shotOf';

const FOLDER = '100-reporting-and-data-view-cohort-report';

test('capture: view cohort report', async ({ page, request }) => {
  test.setTimeout(180_000);
  await page.setViewportSize({ width: 1280, height: 800 });
  const seeded = await seedCast(request, 'David Osei');
  const pat = await seedPat(request);
  await signInWithRetry(page, pat);
  const reports = new AdminReportsPage(page);
  await reports.goto();
  await expect(reports.heading).toBeVisible();
  await expect(reports.updatedAt).toBeVisible({ timeout: 30_000 });
  await reports.cohortTab.click();
  await expect(reports.generateCohort).toBeVisible({ timeout: 30_000 });
  await page.waitForLoadState('networkidle');
  await reports.reportCohort.selectOption(seeded.cohortId);
  await shotOf(page, FOLDER, 'selector', page.locator('.report-selector'), 6);

  await reports.generateCohort.click();
  await expect(reports.cohortReport).toContainText(seeded.cohortName, { timeout: 30_000 });
  await expect(reports.cohortReport).toContainText('1 enrolled', { timeout: 30_000 });
  await expect(reports.cohortReport).toContainText('Program average', { timeout: 30_000 });
  await page.waitForLoadState('networkidle');
  const box = (await reports.cohortReport.boundingBox())!;
  const y = box.y + (await page.evaluate(() => window.scrollY));
  await shotFull(page, FOLDER, 'summary', { x: 250, y: y - 10, width: 1030, height: 280 });
  await shotFull(page, FOLDER, 'detail', { x: 250, y: y + 250, width: 1030, height: 520 });
});
