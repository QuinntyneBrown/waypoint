import { expect, test } from '@playwright/test';
import { AdminReportsPage } from '../e2e/pages';
import { seedCast } from './mentoring-cast';
import { seedPat, signInWithRetry } from './reporting-cast';
import { signInApi } from '../e2e/support/api';
import { shotOf } from './shotOf';

const FOLDER = '101-reporting-and-data-view-staff-report';

test('capture: view staff report', async ({ page, request }) => {
  test.setTimeout(180_000);
  await page.setViewportSize({ width: 1280, height: 800 });
  const seeded = await seedCast(request, 'David Osei');
  const pat = await seedPat(request);
  await signInWithRetry(page, pat);
  const reports = new AdminReportsPage(page);
  await reports.goto();
  await expect(reports.heading).toBeVisible();
  await expect(reports.updatedAt).toBeVisible({ timeout: 30_000 });
  await reports.mentorFilter.selectOption(seeded.mentorId);
  await reports.applyFilters.click();
  await expect(reports.recordCount).toContainText('1 youth record', { timeout: 30_000 });
  await reports.staffTab.click();
  const mentor = reports.staffRow('Grace Adeyemi');
  await expect(mentor).toContainText('Caseload 1');
  await expect(mentor).toContainText('Check-ins 1');
  await expect(mentor).toContainText('Under target');
  await page.waitForLoadState('networkidle');
  await shotOf(page, FOLDER, 'mentor-card', mentor, 24);

  await page.getByRole('heading', { name: 'Staff activity', exact: true }).scrollIntoViewIfNeeded();
  await page.waitForLoadState('networkidle');
  await shotOf(page, FOLDER, 'staff-view', page.getByRole('tabpanel'), 2, 1030, 420);

  // A cohort with no youth.
  const auth = await signInApi(request, pat);
  const cohort = await request.post('/api/cohorts', {
    headers: { Authorization: `Bearer ${auth.accessToken}` },
    data: {
      name: `Winter cohort ${crypto.randomUUID().slice(0, 4)}`,
      trackId: seeded.primaryTrackId,
      leadMentorId: seeded.mentorId,
      startDate: new Date().toISOString(),
      endDate: new Date(Date.now() + 90 * 24 * 60 * 60 * 1000).toISOString(),
      location: 'Reporting room',
    },
  });
  expect(cohort.ok(), await cohort.text()).toBeTruthy();
  const { id } = (await cohort.json()) as { id: string };
  await page.reload();
  await expect(reports.heading).toBeVisible();
  await expect(reports.updatedAt).toBeVisible({ timeout: 30_000 });
  await reports.cohortFilter.selectOption(id);
  await reports.applyFilters.click();
  await reports.staffTab.click();
  await page.waitForLoadState('networkidle');
  await page.waitForTimeout(800);
  await page.screenshot({
    path: `../docs/videos/${FOLDER}/images/empty-1280.png`,
    clip: { x: 250, y: 70, width: 1030, height: 700 },
  });
});
