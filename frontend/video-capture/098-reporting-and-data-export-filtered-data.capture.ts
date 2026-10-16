import { expect, test } from '@playwright/test';
import { AdminAuditLogPage, AdminReportsPage, SignInPage } from '../e2e/pages';
import { seedCast } from './mentoring-cast';
import { seedPat, signInWithRetry } from './reporting-cast';
import { shotFull } from './shotOf';
import { shot } from './shots';

const FOLDER = '098-reporting-and-data-export-filtered-data';
const MAIN = { x: 250, y: 70, width: 1030, height: 560 };

test('capture: export filtered data', async ({ page, request }) => {
  test.setTimeout(180_000);
  await page.setViewportSize({ width: 1280, height: 800 });
  const seeded = await seedCast(request, 'David Osei');
  const pat = await seedPat(request);
  await signInWithRetry(page, pat);
  const reports = new AdminReportsPage(page);
  await reports.goto();
  await expect(reports.heading).toBeVisible();
  await reports.cohortFilter.selectOption(seeded.cohortId);
  await reports.trackFilter.selectOption(seeded.primaryTrackId);
  await reports.mentorFilter.selectOption(seeded.mentorId);
  await reports.applyFilters.click();
  await expect(reports.recordCount).toContainText('1 youth record', { timeout: 30_000 });
  await page.waitForLoadState('networkidle');
  await shotFull(page, FOLDER, 'filtered', { x: 250, y: 70, width: 1030, height: 440 });

  const [csv] = await Promise.all([page.waitForEvent('download'), reports.exportCsv.click()]);
  expect(csv.suggestedFilename()).toMatch(/overview-report.*\.csv$/i);
  await expect(page.getByRole('status').filter({ hasText: 'CSV export prepared' })).toBeVisible();
  await shotFull(page, FOLDER, 'csv', { x: 250, y: 70, width: 1030, height: 440 });

  await reports.youthTab.click();
  await reports.youth.selectOption(seeded.youthId);
  await reports.generateYouth.click();
  await expect(reports.youthReport).toContainText('David Osei');
  const [pdf] = await Promise.all([page.waitForEvent('download'), reports.exportPdf.click()]);
  expect(pdf.suggestedFilename()).toMatch(/youth-report.*\.pdf$/i);
  await expect(page.getByRole('status').filter({ hasText: 'PDF export prepared' })).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shotFull(page, FOLDER, 'pdf', MAIN);

  // The export is recorded in the audit log.
  const audit = new AdminAuditLogPage(page);
  await audit.goto();
  await expect(audit.heading).toBeVisible();
  await audit.userFilter.selectOption(pat.userId);
  await audit.actionFilter.selectOption('DataExported');
  await audit.applyFilters.click();
  await expect(audit.rows).toHaveCount(2);
  await expect(audit.applyFilters).toBeEnabled();
  await page.waitForLoadState('networkidle');
  await shotFull(page, FOLDER, 'audit', { x: 250, y: 70, width: 1030, height: 595 });

  // Other roles are denied the audit log.
  const mentorPage = await page.context().browser()!.newPage({ baseURL: 'http://localhost:4200' });
  await new SignInPage(mentorPage).signIn({
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  await mentorPage.goto(AdminAuditLogPage.path);
  await mentorPage.waitForLoadState('networkidle');
  await shot(mentorPage, FOLDER, 'denied');
});
