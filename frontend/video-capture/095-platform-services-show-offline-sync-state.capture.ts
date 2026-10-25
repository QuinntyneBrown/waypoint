import { expect, test } from '@playwright/test';
import { SignInPage, VolunteerAttendancePage, VolunteerTodayPage } from '../e2e/pages';
import { registerUser, seedVolunteerSession } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '095-platform-services-show-offline-sync-state';
const TOP = { x: 0, y: 0, width: 1280, height: 440 };
const PAGE = { x: 0, y: 0, width: 1280, height: 640 };

test('capture: offline and queued sync', async ({ page, request, context }) => {
  const volunteer = await registerUser(request, 'Volunteer', 'sam-whitfield');
  const seeded = await seedVolunteerSession(request, volunteer.userId);
  await new SignInPage(page).signIn(volunteer);
  await new VolunteerTodayPage(page).takeAttendanceLink.click();
  const attendance = new VolunteerAttendancePage(page);
  await expect(attendance.rosterRows).toHaveCount(seeded.youth.length);
  await attendance.allPresentButton.click();
  await page.waitForLoadState('networkidle');
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'online', TOP);

  const banner = page.locator('.ph-offline');
  await context.setOffline(true);
  await page.evaluate(() => window.dispatchEvent(new Event('offline')));
  await expect(banner).toHaveClass(/is-visible/);
  await page.waitForTimeout(400);
  await shot(page, FOLDER, 'offline-banner', TOP);

  await attendance.markAbsent(seeded.youth[0].name);
  await attendance.saveButton.click();
  await expect(attendance.syncStatus).toContainText(/saved offline.*waiting to sync/i);
  await expect(page.getByText(/pending sync/i)).toBeVisible();
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'pending', PAGE);

  await context.setOffline(false);
  await page.evaluate(() => window.dispatchEvent(new Event('online')));
  await expect(banner).not.toHaveClass(/is-visible/);
  await expect(attendance.syncStatus).toContainText(/all attendance changes synced/i);
  await page.waitForTimeout(400);
  await shot(page, FOLDER, 'synced', PAGE);
});
