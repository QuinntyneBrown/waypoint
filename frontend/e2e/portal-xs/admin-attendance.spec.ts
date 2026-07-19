import { expect, test } from '@playwright/test';
import { AdminAttendanceCapturePage, AdminAttendanceHistoryPage, SignInPage } from '../pages';
import { registerUser, seedYouthCurriculum } from '../support/api';

// Traces to: L2-ENGAGE-002..004, L2-NFR-009, L2-NFR-010
test('admin attendance capture and history remain touch usable on mobile', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'attendance-admin-mobile');
  await new SignInPage(page).signIn(admin);
  await page.goto('/admin/attendance/' + seeded.scheduleSessionId);

  const capture = new AdminAttendanceCapturePage(page);
  const excused = capture.statusButton(seeded.youthName, 'Excused');
  expect((await excused.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await excused.click();
  await capture.save.click();
  await expect(capture.status).toContainText(/attendance saved/i);
  await expect.poll(() => capture.horizontalOverflow()).toBeLessThanOrEqual(1);

  const history = new AdminAttendanceHistoryPage(page);
  await history.goto();
  await history.youthTab.click();
  await expect(history.youthRows.filter({ hasText: seeded.youthName })).toBeVisible();
  expect((await history.exportCsv.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await expect.poll(() => history.horizontalOverflow()).toBeLessThanOrEqual(1);
});
