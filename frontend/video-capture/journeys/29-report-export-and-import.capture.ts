import { expect, test } from '@playwright/test';
import { AdminReportsPage, AdminRosterImportPage } from '../../e2e/pages';
import { seedCast } from '../mentoring-cast';
import { seedPat, signInWithRetry } from '../reporting-cast';
import { shotFull } from '../shotOf';

const FOLDER = 'journeys/29-report-export-and-import';

test('capture: report, export and import', async ({ page, request }) => {
  test.setTimeout(180_000);
  await page.setViewportSize({ width: 1280, height: 900 });
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
  await expect(reports.overview).toContainText('Active youth');
  await page.waitForLoadState('networkidle');
  await shotFull(page, FOLDER, 'filtered', { x: 250, y: 70, width: 1030, height: 700 });

  const [csv] = await Promise.all([page.waitForEvent('download'), reports.exportCsv.click()]);
  expect(csv.suggestedFilename()).toMatch(/overview-report.*\.csv$/i);
  await expect(page.getByRole('status').filter({ hasText: 'CSV export prepared' })).toBeVisible();
  await shotFull(page, FOLDER, 'csv', { x: 250, y: 70, width: 1030, height: 440 });

  const readyEmail = `amara.${crypto.randomUUID()}@example.com`;
  const consentEmail = `imani.${crypto.randomUUID()}@example.com`;
  const csvText = [
    'Full Name,Email Address,Group,Leader,Stream,Notes,Unused',
    `Amara Bell,${readyEmail},${seeded.cohortName},${seeded.mentorName},${seeded.primaryTrackName},Active,ignore me`,
    `David Osei,${seeded.youthEmail},${seeded.cohortName},${seeded.mentorName},${seeded.primaryTrackName},Active,ignore me`,
    `Imani Clarke,${consentEmail},${seeded.cohortName},${seeded.mentorName},${seeded.primaryTrackName},MissingConsent,ignore me`,
    `Broken Email,not-an-email,${seeded.cohortName},${seeded.mentorName},${seeded.primaryTrackName},Active,ignore me`,
  ].join('\n');

  const roster = new AdminRosterImportPage(page);
  await roster.goto();
  await expect(roster.heading).toBeVisible();
  await roster.fileInput.setInputFiles({
    name: 'summer-roster.csv',
    mimeType: 'text/csv',
    buffer: Buffer.from(csvText),
  });
  await expect(roster.fileSummary).toContainText('4 rows · 7 columns');
  await roster.mapColumn('Notes').selectOption('Status');
  await roster.mapColumn('Leader').selectOption('Skip');
  await page.waitForLoadState('networkidle');
  await shotFull(page, FOLDER, 'mapping', { x: 250, y: 272, width: 1030, height: 800 });

  await roster.previewButton.click();
  await expect(roster.previewSummary).toContainText('1 ready');
  await expect(roster.previewRow(seeded.youthEmail)).toContainText('Duplicate');
  await expect(roster.previewRow(consentEmail)).toContainText('Missing consent');
  await page.waitForLoadState('networkidle');
  await roster.previewSummary.scrollIntoViewIfNeeded();
  const top = (await roster.previewSummary.boundingBox())!;
  await shotFull(page, FOLDER, 'preview', {
    x: 250,
    y: Math.max(0, top.y + (await page.evaluate(() => window.scrollY)) - 80),
    width: 1030,
    height: 520,
  });

  await roster.importReady.click();
  await expect(roster.confirmation).toContainText('Only 1 ready row will create an account');
  await roster.confirmImport.click();
  await expect(roster.result).toContainText('Import complete');
  await expect(roster.result).toContainText('1 imported');
  await page.waitForLoadState('networkidle');
  await roster.result.scrollIntoViewIfNeeded();
  const res = (await roster.result.boundingBox())!;
  await shotFull(page, FOLDER, 'result', {
    x: 250,
    y: res.y + (await page.evaluate(() => window.scrollY)) - 4,
    width: 1030,
    height: res.height + 8,
  });
});
