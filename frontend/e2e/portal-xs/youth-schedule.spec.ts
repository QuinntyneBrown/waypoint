import { expect, test } from '@playwright/test';
import { SignInPage, YouthSchedulePage } from '../pages';
import { seedYouthCurriculum } from '../support/api';

// Traces to: L2-ENGAGE-005, L2-NFR-009, L2-NFR-010
test('schedule and RSVP controls remain contained, touch-sized, and keyboard operable on mobile', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  await page.goto('/youth/schedule');

  const schedule = new YouthSchedulePage(page);
  await expect(schedule.sessions).toContainText(seeded.scheduleSessionTitle);
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);

  for (const control of [
    schedule.goingOption,
    schedule.maybeOption,
    schedule.cantMakeItOption,
    schedule.reminderOption,
    schedule.saveButton,
  ]) {
    expect((await control.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  }

  await schedule.going.focus();
  await page.keyboard.press('Space');
  await expect(schedule.going).toBeChecked();
  await schedule.mentorNote.focus();
  await page.keyboard.type('Keyboard RSVP note');
  await schedule.reminder.focus();
  await page.keyboard.press('Space');
  await schedule.saveButton.focus();
  await page.keyboard.press('Enter');
  await expect(schedule.status).toContainText('RSVP saved.');
  await expect(schedule.currentResponse).toContainText('Going');
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);
});
