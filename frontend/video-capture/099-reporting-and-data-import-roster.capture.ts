import { expect, test } from '@playwright/test';
import { AdminRosterImportPage } from '../e2e/pages';
import { seedCast } from './mentoring-cast';
import { seedPat, signInWithRetry } from './reporting-cast';
import { shotFull } from './shotOf';

const FOLDER = '099-reporting-and-data-import-roster';
const TOP = { x: 250, y: 70, width: 1030, height: 700 };

test('capture: import roster', async ({ page, request }) => {
  test.setTimeout(180_000);
  await page.setViewportSize({ width: 1280, height: 900 });
  const seeded = await seedCast(request, 'David Osei');
  const pat = await seedPat(request);
  const readyEmail = `amara.${crypto.randomUUID()}@example.com`;
  const consentEmail = `imani.${crypto.randomUUID()}@example.com`;
  const csv = [
    'Full Name,Email Address,Group,Leader,Stream,Notes,Unused',
    `Amara Bell,${readyEmail},${seeded.cohortName},${seeded.mentorName},${seeded.primaryTrackName},Active,ignore me`,
    `David Osei,${seeded.youthEmail},${seeded.cohortName},${seeded.mentorName},${seeded.primaryTrackName},Active,ignore me`,
    `Imani Clarke,${consentEmail},${seeded.cohortName},${seeded.mentorName},${seeded.primaryTrackName},MissingConsent,ignore me`,
    `Broken Email,not-an-email,${seeded.cohortName},${seeded.mentorName},${seeded.primaryTrackName},Active,ignore me`,
  ].join('\n');

  await signInWithRetry(page, pat);
  const roster = new AdminRosterImportPage(page);
  await roster.goto();
  await expect(roster.heading).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shotFull(page, FOLDER, 'upload', { x: 250, y: 70, width: 1030, height: 420 });

  await roster.fileInput.setInputFiles({
    name: 'roster.txt',
    mimeType: 'text/plain',
    buffer: Buffer.from('name,email\nAlice,alice@example.com'),
  });
  await expect(roster.fileError).toContainText('Choose a .csv file');
  await shotFull(page, FOLDER, 'wrong-type', { x: 250, y: 70, width: 1030, height: 480 });

  await roster.fileInput.setInputFiles({
    name: 'summer-roster.csv',
    mimeType: 'text/csv',
    buffer: Buffer.from(csv),
  });
  await expect(roster.fileSummary).toContainText('4 rows · 7 columns');
  await roster.mapColumn('Notes').selectOption('Status');
  await roster.mapColumn('Leader').selectOption('Skip');
  await expect(roster.mapColumn('Unused')).toHaveValue('Skip');
  await page.waitForLoadState('networkidle');
  await shotFull(page, FOLDER, 'mapping', { x: 250, y: 272, width: 1030, height: 800 });

  await roster.previewButton.click();
  await expect(roster.previewSummary).toContainText('1 ready');
  await expect(roster.previewRow(readyEmail)).toContainText('Ready');
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
  await page.waitForLoadState('networkidle');
  await page.screenshot({
    path: `../docs/videos/${FOLDER}/images/confirm-1280.png`,
    clip: { x: 370, y: 275, width: 540, height: 250 },
  });

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
