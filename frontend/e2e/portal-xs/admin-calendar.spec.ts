import { expect, test } from '@playwright/test';
import { AdminCalendarPage, SignInPage } from '../pages';
import { registerUser, seedYouthCurriculum } from '../support/api';

// Traces to: L2-ENGAGE-001, L2-NFR-009, L2-NFR-010
test('the admin calendar keeps its views and session form usable on mobile', async ({
  page,
  request,
}) => {
  await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'program-calendar-mobile');
  await new SignInPage(page).signIn(admin);
  const calendar = new AdminCalendarPage(page);
  await calendar.goto();

  await expect(calendar.heading).toBeVisible();
  expect((await calendar.schedule.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await calendar.weekTab.click();
  await expect(calendar.week).toBeVisible();
  await calendar.agendaTab.click();
  await expect(calendar.agenda).toBeVisible();
  await expect.poll(() => calendar.horizontalOverflow()).toBeLessThanOrEqual(1);
});
