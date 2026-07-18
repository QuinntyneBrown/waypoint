import { expect, test } from '@playwright/test';
import { SignInPage, VolunteerAttendancePage, VolunteerTodayPage } from '../pages';
import { registerUser, seedVolunteerSession } from '../support/api';

// Traces to: L2-NFR-009, L2-NFR-010, L2-VOLUNTEER-001
test('volunteer attendance remains operable and contained on a mobile viewport', async ({
  page,
  request,
}) => {
  const volunteer = await registerUser(request, 'Volunteer', 'vol-mobile-attendance');
  const seeded = await seedVolunteerSession(request, volunteer.userId);

  await new SignInPage(page).signIn(volunteer);
  await new VolunteerTodayPage(page).takeAttendanceLink.click();
  const attendance = new VolunteerAttendancePage(page);
  await expect(attendance.rosterRows).toHaveCount(seeded.youth.length);

  const overflow = await page.evaluate(() => document.documentElement.scrollWidth - innerWidth);
  expect(overflow).toBeLessThanOrEqual(1);

  const markButtons = attendance.rosterRows.getByRole('button');
  await expect(markButtons).toHaveCount(seeded.youth.length * 2);
  for (const button of await markButtons.all()) {
    const box = await button.boundingBox();
    expect(box?.height).toBeGreaterThanOrEqual(44);
    expect(box?.width).toBeGreaterThanOrEqual(44);
  }

  await attendance.allPresentButton.focus();
  await page.keyboard.press('Enter');
  await expect(attendance.markedTally).toContainText(
    `${seeded.youth.length} of ${seeded.youth.length}`,
  );
});
