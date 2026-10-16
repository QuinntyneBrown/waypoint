import { expect, test } from '@playwright/test';
import { SignInPage, VolunteerAttendancePage, VolunteerTodayPage } from '../e2e/pages';
import { registerUser, seedVolunteerSession } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '064-volunteer-portal-record-session-attendance';
const PAGE = { x: 362, y: 0, width: 800, height: 660 };

test('capture: record session attendance', async ({ page, request, context }) => {
  const volunteer = await registerUser(request, 'Volunteer', 'sam-whitfield');
  const seeded = await seedVolunteerSession(request, volunteer.userId);

  await new SignInPage(page).signIn(volunteer);
  await new VolunteerTodayPage(page).takeAttendanceLink.click();
  const attendance = new VolunteerAttendancePage(page);
  await expect(attendance.rosterRows).toHaveCount(seeded.youth.length);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'roster', PAGE);

  await attendance.allPresentButton.click();
  await attendance.markAbsent(seeded.youth[1].name);
  await expect(attendance.markedTally).toContainText(
    `${seeded.youth.length} of ${seeded.youth.length}`,
  );
  await shot(page, FOLDER, 'marked', PAGE);

  await attendance.saveButton.click();
  await expect(attendance.syncStatus).toContainText(/attendance saved/i);
  await shot(page, FOLDER, 'saved', PAGE);

  // Offline: a second pass is queued and syncs on reconnect.
  await context.setOffline(true);
  await attendance.markAbsent(seeded.youth[0].name);
  await attendance.saveButton.click();
  await expect(attendance.syncStatus).toContainText(/saved offline.*waiting to sync/i);
  await shot(page, FOLDER, 'offline', PAGE);

  await context.setOffline(false);
  await expect(attendance.syncStatus).toContainText(/all attendance changes synced/i);
  await shot(page, FOLDER, 'synced', PAGE);
});
