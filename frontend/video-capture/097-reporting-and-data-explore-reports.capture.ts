import { expect, test } from '@playwright/test';
import { AdminReportsPage } from '../e2e/pages';
import { signInApi } from '../e2e/support/api';
import { seedCast } from './mentoring-cast';
import { seedPat, signInWithRetry } from './reporting-cast';
import { shot } from './shots';
import { shotFull } from './shotOf';

const FOLDER = '097-reporting-and-data-explore-reports';
const MAIN = { x: 250, y: 70, width: 1030, height: 700 };

test('capture: explore reports', async ({ page, request }) => {
  test.setTimeout(180_000);
  await page.setViewportSize({ width: 1280, height: 800 });
  const seeded = await seedCast(request, 'David Osei');
  const pat = await seedPat(request);
  await signInWithRetry(page, pat);
  const reports = new AdminReportsPage(page);
  await reports.goto();
  await expect(reports.heading).toBeVisible();

  const from = new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().slice(0, 10);
  const to = new Date().toISOString().slice(0, 10);
  await reports.from.fill(from);
  await reports.to.fill(to);
  await reports.cohortFilter.selectOption(seeded.cohortId);
  await reports.trackFilter.selectOption(seeded.primaryTrackId);
  await reports.mentorFilter.selectOption(seeded.mentorId);
  await reports.applyFilters.click();
  await expect(reports.recordCount).toContainText('1 youth record');
  await expect(reports.overview).toContainText('Active youth');
  await page.waitForLoadState('networkidle');
  await shotFull(page, FOLDER, 'filtered', MAIN);

  // A real empty scope: a cohort with no youth.
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
  await reports.cohortFilter.selectOption(id);
  await reports.applyFilters.click();
  await expect(
    page.getByRole('heading', { name: 'No report data for these filters' }),
  ).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shotFull(page, FOLDER, 'empty', MAIN);

  await page.route(
    '**/api/reports/hub?*',
    (route) => route.fulfill({ status: 500, contentType: 'application/problem+json', body: '{}' }),
    { times: 1 },
  );
  await reports.applyFilters.click();
  await expect(
    page
      .getByRole('alert')
      .filter({ hasText: 'Report data could not be loaded' })
      .or(page.getByRole('heading', { name: 'Something went wrong' })),
  ).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'error');
});
