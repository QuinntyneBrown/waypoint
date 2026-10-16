import { expect, Page, test } from '@playwright/test';
import { NotificationsSettingsPage, SignInPage } from '../e2e/pages';
import { shot } from './shots';

/** Sign in through the real form, allowing for slow password hashing on a busy shared API. */
async function signInSlow(page: Page, email: string, password: string) {
  const signIn = new SignInPage(page);
  await signIn.goto();
  await signIn.submitCredentials(email, password);
  await expect(page).not.toHaveURL(/\/auth\/sign-in/, { timeout: 30000 });
}

const FOLDER = '085-platform-services-configure-notifications';
const CONTENT = { x: 244, y: 80, width: 1036, height: 720 };

test('capture: configure notifications', async ({ page, request }) => {
  test.setTimeout(240000);
  const email = `grace-${crypto.randomUUID().slice(0, 6)}@waypoint.demo`;
  const password = 'Password123';
  const seeded = await request.post('/api/_test/users', {
    data: { email, password, displayName: 'Grace Adeyemi', role: 'Mentor' },
  });
  expect(seeded.ok()).toBeTruthy();

  await signInSlow(page, email, password);
  const notifications = new NotificationsSettingsPage(page);
  await notifications.goto();
  await expect(notifications.checkbox('Work graded in-app')).toBeAttached();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'page', CONTENT);

  await page.getByText('Program-wide news and updates.').scrollIntoViewIfNeeded();
  await expect(notifications.checkbox('Announcements in-app')).not.toBeChecked();
  await shot(page, FOLDER, 'before', CONTENT);

  await notifications.toggleCheck('Announcements in-app');
  await expect(notifications.checkbox('Announcements in-app')).toBeChecked();
  await expect(notifications.savedToast).toBeVisible({ timeout: 30000 });
  await page.getByText('Program-wide news and updates.').scrollIntoViewIfNeeded();
  await shot(page, FOLDER, 'saved');

  await page.reload();
  await page.waitForLoadState('networkidle');
  await expect(notifications.checkbox('Announcements in-app')).toBeChecked();
  await page.getByText('Program-wide news and updates.').scrollIntoViewIfNeeded();
  await shot(page, FOLDER, 'reloaded', CONTENT);
});
