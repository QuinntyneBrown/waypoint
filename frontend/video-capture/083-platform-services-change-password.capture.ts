import { expect, Page, test } from '@playwright/test';
import { ProfileSettingsPage, SignInPage } from '../e2e/pages';
import { shot } from './shots';

/** Sign in through the real form, allowing for slow password hashing on a busy shared API. */
async function signInSlow(page: Page, email: string, password: string) {
  const signIn = new SignInPage(page);
  await signIn.goto();
  await signIn.submitCredentials(email, password);
  await expect(page).not.toHaveURL(/\/auth\/sign-in/, { timeout: 30000 });
}

const FOLDER = '083-platform-services-change-password';

/** Scroll the password section into view and crop to it plus any toast above. */
async function passwordArea(page: Page) {
  const heading = page.getByRole('heading', { name: 'Change password' });
  await heading.evaluate((el) => el.scrollIntoView({ block: 'start' }));
  const box = (await heading.boundingBox())!;
  return { x: 244, y: Math.max(box.y - 20, 0), width: 1036, height: 800 - Math.max(box.y - 20, 0) };
}

test('capture: change password', async ({ page, request }) => {
  test.setTimeout(240000);
  const email = `david-${crypto.randomUUID().slice(0, 6)}@waypoint.demo`;
  const password = 'Password123';
  const displayName = 'David Osei';
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
  await expect(profile.currentPassword).toBeVisible();
  await page.waitForLoadState('networkidle');

  await profile.currentPassword.fill(password);
  await profile.newPassword.fill('short');
  await profile.confirmNewPassword.fill('short');
  await expect(profile.passwordPolicyHint).toBeVisible();
  await expect(profile.changePasswordButton).toBeDisabled();
  await shot(page, FOLDER, 'policy', await passwordArea(page));

  await profile.newPassword.fill('NewPassword9');
  await profile.confirmNewPassword.fill('Mismatch9');
  await expect(profile.passwordMismatchError).toBeVisible();
  await shot(page, FOLDER, 'mismatch', await passwordArea(page));

  await profile.confirmNewPassword.fill('NewPassword9');
  await expect(profile.changePasswordButton).toBeEnabled();
  await shot(page, FOLDER, 'ready', await passwordArea(page));

  await profile.changePasswordButton.click();
  await expect(page.getByText('Password changed.')).toBeVisible({ timeout: 30000 });
  await shot(page, FOLDER, 'changed');
});
