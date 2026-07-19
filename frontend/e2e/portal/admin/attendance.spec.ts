import { expect, test } from '@playwright/test';
import { AdminAttendanceCapturePage, AdminAttendanceHistoryPage, SignInPage } from '../../pages';
import { registerUser, seedYouthCurriculum, signInApi } from '../../support/api';

// Traces to: L2-ENGAGE-002..004, L2-NFR-004/006/007, L2-REPORT-003
test('an admin edits attendance on behalf of staff and exports organization history', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'attendance-admin');
  await new SignInPage(page).signIn(admin);
  await page.goto('/admin/attendance/' + seeded.scheduleSessionId);

  const capture = new AdminAttendanceCapturePage(page);
  await expect(capture.heading).toContainText(seeded.scheduleSessionTitle);
  await expect(capture.rows).toHaveCount(1);
  await expect(capture.calendar).toHaveAttribute('href', '/admin/calendar');
  await capture.statusButton(seeded.youthName, 'Late').click();
  await capture.note(seeded.youthName).fill('Director correction after mentor follow-up.');
  await expect(capture.markedTally).toContainText('1 of 1');
  await capture.save.click();
  await expect(capture.status).toContainText(/attendance saved/i);

  const auth = await signInApi(request, admin);
  const roster = await request.get('/api/attendance/' + seeded.scheduleSessionId + '/roster', {
    headers: { Authorization: 'Bearer ' + auth.accessToken },
  });
  expect(roster.ok(), await roster.text()).toBeTruthy();
  expect(await roster.text()).toContain('Director correction after mentor follow-up.');

  const history = new AdminAttendanceHistoryPage(page);
  await history.goto();
  await expect(history.heading).toBeVisible();
  const through = new Date();
  through.setDate(through.getDate() + 10);
  await history.cohort.selectOption(seeded.cohortId);
  await history.to.fill(through.toISOString().slice(0, 10));
  await history.applyFilters.click();
  const session = history.sessionRows.filter({ hasText: seeded.scheduleSessionTitle });
  await expect(session).toContainText('saved');
  await expect(session.getByRole('link', { name: 'Edit', exact: true })).toHaveAttribute(
    'href',
    '/admin/attendance/' + seeded.scheduleSessionId,
  );

  const csvDownload = page.waitForEvent('download');
  await history.exportCsv.click();
  expect((await csvDownload).suggestedFilename()).toMatch(/attendance-session.*\.csv/i);
  await history.youthTab.click();
  const youth = history.youthRows.filter({ hasText: seeded.youthName });
  await expect(youth.getByRole('cell').nth(2)).toHaveText('1');
  await expect(youth).toContainText(/improving|declining|steady|no data/i);
  const pdfDownload = page.waitForEvent('download');
  await history.exportPdf.click();
  expect((await pdfDownload).suggestedFilename()).toMatch(/attendance-youth.*\.pdf/i);
});
