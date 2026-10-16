import { expect, Page, test } from '@playwright/test';
import {
  DisplaySettingsPage,
  NotificationsSettingsPage,
  ProfileSettingsPage,
  SignInPage,
} from '../../e2e/pages';
import { shot } from '../shots';

const FOLDER = 'journeys/32-manage-personal-settings';
const CONTENT = { x: 244, y: 80, width: 1036, height: 720 };

/** Sign in through the real form, allowing for slow password hashing on a busy shared API. */
async function signInSlow(page: Page, email: string, password: string) {
  const signIn = new SignInPage(page);
  await signIn.goto();
  await signIn.submitCredentials(email, password);
  await expect(page).not.toHaveURL(/\/auth\/sign-in/, { timeout: 30000 });
}

test('capture: manage personal settings journey', async ({ page, request }) => {
  test.setTimeout(240000);
  const email = `amara-${crypto.randomUUID()}@waypoint.demo`;
  const password = 'Password123';
  const displayName = 'Amara Bell';
  const registered = await request.post('/api/auth/register', {
    data: { email, password, displayName },
  });
  expect(registered.ok()).toBeTruthy();
  const auth = (await registered.json()) as { accessToken: string };
  await request.post('/api/onboarding', {
    headers: { Authorization: `Bearer ${auth.accessToken}` },
    data: { displayName },
  });

  await signInSlow(page, email, password);

  const profile = new ProfileSettingsPage(page);
  await profile.goto();
  await expect(profile.displayName).toHaveValue(displayName);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'profile', CONTENT);
  await profile.pronouns.fill('she/her');
  await profile.saveProfileButton.click();
  await expect(profile.profileUpdatedToast).toBeVisible({ timeout: 30000 });
  await shot(page, FOLDER, 'profile-saved');

  const notifications = new NotificationsSettingsPage(page);
  await notifications.goto();
  await expect(notifications.checkbox('Work graded in-app')).toBeAttached();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'notifications', CONTENT);

  await page.getByText('Program-wide news and updates.').scrollIntoViewIfNeeded();
  await notifications.toggleCheck('Announcements in-app');
  await expect(notifications.checkbox('Announcements in-app')).toBeChecked();
  await expect(notifications.savedToast).toBeVisible({ timeout: 30000 });
  await page.getByText('Program-wide news and updates.').scrollIntoViewIfNeeded();
  await shot(page, FOLDER, 'notifications-saved');

  const display = new DisplaySettingsPage(page);
  await display.goto();
  await expect(display.increaseTextSizeButton).toBeVisible();
  await page.waitForLoadState('networkidle');
  await display.increaseTextSizeButton.click();
  await display.increaseTextSizeButton.click();
  await expect(display.textScaleReadout).toHaveText('120%');
  await display.selectTheme('Dark');
  await expect.poll(() => display.rootTheme()).toBe('dark');
  await shot(page, FOLDER, 'display', CONTENT);

  await display.save();
  await expect(display.savedToast).toBeVisible({ timeout: 30000 });
  await page.reload();
  await page.waitForLoadState('networkidle');
  await expect(display.textScaleReadout).toHaveText('120%');
  console.log('THEME AFTER RELOAD', await display.rootTheme());
});
