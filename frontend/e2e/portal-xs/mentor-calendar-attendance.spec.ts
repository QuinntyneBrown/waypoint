import { expect, test } from '@playwright/test';
import {
  MentorAttendanceCapturePage,
  MentorAttendanceHistoryPage,
  MentorCalendarPage,
  SignInPage,
} from '../pages';
import { seedYouthCurriculum } from '../support/api';

// Traces to: L2-ENGAGE-001..004, L2-NFR-009/010
test('mentor calendar and four-state attendance remain keyboard usable on mobile', async ({
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
  await calendar.weekTab.focus();
  await page.keyboard.press('Enter');
  await expect(calendar.week).toBeVisible();
  await calendar.agendaTab.focus();
  await page.keyboard.press('Enter');
  await expect(calendar.agenda).toContainText(seeded.scheduleSessionTitle);
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);

  await page.goto(`/mentor/attendance/${seeded.scheduleSessionId}`);
  const capture = new MentorAttendanceCapturePage(page);
  const late = capture.statusButton(seeded.youthName, 'Late');
  expect((await late.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await late.focus();
  await page.keyboard.press('Enter');
  await capture.note(seeded.youthName).focus();
  await page.keyboard.type('Mobile attendance note.');
  await capture.save.focus();
  await page.keyboard.press('Enter');
  await expect(capture.status).toContainText(/attendance saved/i);
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);

  await page.goto('/mentor/attendance');
  const history = new MentorAttendanceHistoryPage(page);
  await history.youthTab.focus();
  await page.keyboard.press('Enter');
  await expect(history.youthRows.filter({ hasText: seeded.youthName })).toBeVisible();
  expect((await history.exportCsv.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);
});
