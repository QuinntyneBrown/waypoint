import { expect, test } from '@playwright/test';
import {
  MentorAttendanceCapturePage,
  MentorAttendanceHistoryPage,
  MentorCalendarPage,
  SignInPage,
} from '../../pages';
import { seedYouthCurriculum, signInApi } from '../../support/api';

// Traces to: L2-ENGAGE-001..004, L2-NFR-004/006/007, L2-REPORT-003, L2-AUTH-010
test('mentor schedules recurring sessions, captures attendance, and exports filtered history', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });

  await page.goto('/mentor/calendar');
  const calendar = new MentorCalendarPage(page);
  await expect(calendar.heading).toBeVisible();
  await expect(calendar.month).toBeVisible();
  await calendar.weekTab.click();
  await expect(calendar.week).toBeVisible();
  await calendar.agendaTab.click();
  await expect(calendar.agenda).toContainText(seeded.scheduleSessionTitle);

  const sessionTitle = `Mentor E2E circle ${crypto.randomUUID().slice(0, 8)}`;
  const sessionDate = new Date();
  sessionDate.setDate(sessionDate.getDate() + 2);
  await calendar.title.fill(sessionTitle);
  await calendar.cohort.selectOption(seeded.cohortId);
  await calendar.mentor.selectOption(seeded.mentorId);
  await calendar.date.fill(sessionDate.toISOString().slice(0, 10));
  await calendar.time.fill('18:30');
  await calendar.location.fill('E2E Studio');
  await calendar.repeatsWeeklyControl.click();
  await expect(calendar.repeatsWeekly).toBeChecked();
  await calendar.schedule.click();
  await expect(calendar.status).toContainText(/weekly sessions scheduled through the cohort term/i);
  await expect(calendar.agenda).toContainText(sessionTitle);

  await page.goto(`/mentor/attendance/${seeded.scheduleSessionId}`);
  const capture = new MentorAttendanceCapturePage(page);
  await expect(capture.heading).toContainText(seeded.scheduleSessionTitle);
  await expect(capture.rows).toHaveCount(1);
  await capture.statusButton(seeded.youthName, 'Late').click();
  await capture.note(seeded.youthName).fill('Transit delay; joined ready to participate.');
  await capture.save.click();
  await expect(capture.status).toContainText(/attendance saved/i);

  const auth = await signInApi(request, {
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  const roster = await request.get(`/api/attendance/${seeded.scheduleSessionId}/roster`, {
    headers: { Authorization: `Bearer ${auth.accessToken}` },
  });
  expect(roster.ok(), await roster.text()).toBeTruthy();
  const rosterBody = await roster.text();
  expect(rosterBody).toContain('Late');
  expect(rosterBody).toContain('Transit delay');

  await page.goto('/mentor/attendance');
  const history = new MentorAttendanceHistoryPage(page);
  await expect(history.heading).toBeVisible();
  const through = new Date();
  through.setDate(through.getDate() + 10);
  await history.to.fill(through.toISOString().slice(0, 10));
  await history.applyFilters.click();
  await expect(history.sessionRows.filter({ hasText: seeded.scheduleSessionTitle })).toContainText(
    'saved',
  );

  const csvDownload = page.waitForEvent('download');
  await history.exportCsv.click();
  expect((await csvDownload).suggestedFilename()).toMatch(/attendance-session.*\.csv/i);

  await history.youthTab.click();
  const youthRow = history.youthRows.filter({ hasText: seeded.youthName });
  await expect(youthRow).toContainText('Below 70%');
  await expect(youthRow).toContainText('improving');
  const pdfDownload = page.waitForEvent('download');
  await history.exportPdf.click();
  expect((await pdfDownload).suggestedFilename()).toMatch(/attendance-youth.*\.pdf/i);
});

test('mentor attendance uses the private offline queue and automatically syncs', async ({
  page,
  context,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const auth = await signInApi(request, {
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  await new SignInPage(page).signIn({
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  await page.goto(`/mentor/attendance/${seeded.scheduleSessionId}`);

  const capture = new MentorAttendanceCapturePage(page);
  await expect(capture.rows).toHaveCount(1);
  await context.setOffline(true);
  await capture.allPresent.click();
  await capture.note(seeded.youthName).fill('Queued mentor note.');
  await capture.save.click();
  await expect(capture.status).toContainText(/saved offline.*1.*waiting to sync/i);

  const queued = await page.evaluate(() =>
    Object.entries(localStorage)
      .filter(([key]) => key.startsWith('w.attendance.queue.'))
      .map(([, value]) => JSON.parse(value)),
  );
  expect(JSON.stringify(queued)).toContain(auth.userId);
  expect(JSON.stringify(queued)).not.toContain(seeded.youthName);

  await context.setOffline(false);
  await expect(capture.status).toContainText(/all attendance changes synced/i);
  const roster = await request.get(`/api/attendance/${seeded.scheduleSessionId}/roster`, {
    headers: { Authorization: `Bearer ${auth.accessToken}` },
  });
  expect(roster.ok(), await roster.text()).toBeTruthy();
  expect(await roster.text()).toContain('Queued mentor note');
});
