import { expect, test } from '@playwright/test';
import { AdminAttendanceCapturePage, AdminAttendanceHistoryPage, SignInPage } from '../e2e/pages';
import { seedEngagementCast, seedPat } from './engagement-cast';
import { shot } from './shots';

const FOLDER = '072-engagement-review-attendance';
const HISTORY = { x: 255, y: 215, width: 1015, height: 520 };

test.setTimeout(120_000);
test('capture: review attendance', async ({ page, request }) => {
  const cast = await seedEngagementCast(request);
  const pat = await seedPat(request);
  await new SignInPage(page).signIn(pat);
  await page.goto(`/admin/attendance/${cast.scheduleSessionId}`);
  const capture = new AdminAttendanceCapturePage(page);
  await expect(capture.rows).toHaveCount(1);
  await capture.statusButton(cast.youthName, 'Late').click();
  await capture.save.click();
  await expect(capture.status).toContainText(/attendance saved/i);

  const history = new AdminAttendanceHistoryPage(page);
  await history.goto();
  await expect(history.heading).toBeVisible();
  const through = new Date();
  through.setDate(through.getDate() + 10);
  await history.cohort.selectOption(cast.cohortId);
  await history.to.fill(through.toISOString().slice(0, 10));
  await history.applyFilters.click();
  await expect(history.sessionRows.filter({ hasText: cast.scheduleSessionTitle })).toContainText(
    'saved',
  );
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'by-session', HISTORY);

  await history.youthTab.click();
  await expect(history.youthRows.filter({ hasText: cast.youthName })).toBeVisible();
  await shot(page, FOLDER, 'by-youth', HISTORY);
});
