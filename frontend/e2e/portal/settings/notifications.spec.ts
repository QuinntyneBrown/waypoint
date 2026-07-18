import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { NotificationsSettingsPage, SignInPage } from '../../pages';

// Traces to: L2-PLATFORM-003
test('notification preferences persist a toggled category across reloads', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'notifprefs');
  await new SignInPage(page).signIn(creds);

  const notifications = new NotificationsSettingsPage(page);
  await notifications.goto();

  const announce = notifications.checkbox('Announcements in-app');
  await expect(announce).not.toBeChecked(); // default off

  // Toggle via the label (custom check hides the native input) and await the save.
  await notifications.toggleCheck('Announcements in-app');
  await expect(announce).toBeChecked();
  await expect(notifications.savedToast).toBeVisible();

  await page.reload();
  await page.waitForLoadState('networkidle');
  await expect(notifications.checkbox('Announcements in-app')).toBeChecked();
});
