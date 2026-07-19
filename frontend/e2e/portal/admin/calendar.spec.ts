import { expect, test } from '@playwright/test';
import { AdminCalendarPage, SignInPage } from '../../pages';
import { registerUser, seedYouthCurriculum } from '../../support/api';

// Traces to: L2-ENGAGE-001, L2-AUTH-010
test('an admin filters all sessions and schedules recurrence through the cohort term', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'program-calendar');
  await new SignInPage(page).signIn(admin);
  const calendar = new AdminCalendarPage(page);
  await calendar.goto();

  await expect(calendar.heading).toBeVisible();
  await expect(calendar.month).toBeVisible();
  await calendar.weekTab.click();
  await expect(calendar.week).toBeVisible();
  await calendar.agendaTab.click();
  const seededSession = calendar.session(seeded.scheduleSessionTitle);
  await expect(seededSession).toContainText(seeded.cohortName);
  await expect(seededSession).toContainText(seeded.mentorName);
  await expect(seededSession).toContainText(seeded.scheduleSessionLocation);
  await expect(
    seededSession.getByRole('link', { name: /take attendance|edit attendance/i }),
  ).toHaveAttribute('href', '/admin/attendance/' + seeded.scheduleSessionId);

  await calendar.filter.selectOption(seeded.cohortId);
  await expect(calendar.session(seeded.scheduleSessionTitle)).toBeVisible();

  const title = 'Admin weekly circle ' + crypto.randomUUID().slice(0, 8);
  const starts = new Date();
  starts.setDate(starts.getDate() + 2);
  await calendar.title.fill(title);
  await calendar.cohort.selectOption(seeded.cohortId);
  await calendar.mentor.selectOption(seeded.mentorId);
  await calendar.date.fill(starts.toISOString().slice(0, 10));
  await calendar.time.fill('17:30');
  await calendar.location.fill('Program Studio');
  await calendar.repeatsWeeklyControl.click();
  await expect(calendar.repeatsWeekly).toBeChecked();
  await calendar.schedule.click();
  await expect(calendar.status).toContainText(/weekly sessions scheduled through the cohort term/i);
  const created = calendar.session(title).first();
  await expect(created).toContainText('Program Studio');
  await expect(created).toContainText(seeded.mentorName);
});
