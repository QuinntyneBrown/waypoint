import { expect, test } from '@playwright/test';
import { SignInPage, YouthSchedulePage } from '../../pages';
import { seedYouthCurriculum } from '../../support/api';

// Traces to: L2-ENGAGE-005, L2-ENGAGE-008
test('youth sees upcoming logistics and can save and revise an RSVP with a reminder', async ({
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
  await expect(schedule.heading).toBeVisible();
  await expect(schedule.sessions).toHaveCount(1);
  await expect(schedule.sessions).toContainText(seeded.scheduleSessionTitle);
  await expect(schedule.sessions).toContainText(seeded.scheduleSessionLocation);

  await schedule.maybeOption.click();
  await schedule.mentorNote.fill('I may arrive a few minutes late.');
  await schedule.reminderOption.click();
  await schedule.saveButton.click();
  await expect(schedule.status).toContainText('RSVP saved.');
  await expect(schedule.currentResponse).toContainText('Maybe');
  await expect(schedule.reminderTime).toBeVisible();

  await page.reload();
  await expect(schedule.maybe).toBeChecked();
  await expect(schedule.mentorNote).toHaveValue('I may arrive a few minutes late.');
  await expect(schedule.reminder).toBeChecked();

  await schedule.cantMakeItOption.click();
  await expect(schedule.reminder).not.toBeChecked();
  await expect(schedule.reminder).toBeDisabled();
  await schedule.saveButton.click();
  await expect(schedule.status).toContainText('RSVP saved.');
  await expect(schedule.currentResponse).toContainText('Can’t make it');
});
