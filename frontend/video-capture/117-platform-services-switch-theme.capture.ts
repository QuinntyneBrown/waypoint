import { expect, Page, test } from '@playwright/test';
import { DisplaySettingsPage, SignInPage } from '../e2e/pages';
import { shot } from './shots';

/** Sign in through the real form, allowing for slow password hashing on a busy shared API. */
async function signInSlow(page: Page, email: string, password: string) {
  const signIn = new SignInPage(page);
  await signIn.goto();
  await signIn.submitCredentials(email, password);
  await expect(page).not.toHaveURL(/\/auth\/sign-in/, { timeout: 30000 });
}

const FOLDER = '117-platform-services-switch-theme';
const CONTENT = { x: 244, y: 80, width: 1036, height: 720 };

test('capture: switch theme', async ({ page, request }) => {
  test.setTimeout(240000);
  const email = `amara-${crypto.randomUUID().slice(0, 6)}@waypoint.demo`;
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

  // Signed out: the page follows the device colour scheme.
  const signIn = new SignInPage(page);
  await page.emulateMedia({ colorScheme: 'light' });
  await signIn.goto();
  expect(await signIn.colorScheme()).toBe('light');
  await shot(page, FOLDER, 'device-light');
  await page.emulateMedia({ colorScheme: 'dark' });
  expect(await signIn.colorScheme()).toBe('dark');
  await shot(page, FOLDER, 'device-dark');

  // Signed in on a light device: System follows it.
  await page.emulateMedia({ colorScheme: 'light' });
  await signInSlow(page, email, password);
  const display = new DisplaySettingsPage(page);
  await display.goto();
  await expect(display.themeOption('System')).toHaveAttribute('aria-pressed', 'true');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'system', CONTENT);

  // Dark previews at once, without saving.
  await display.selectTheme('Dark');
  await expect(display.themeOption('Dark')).toHaveAttribute('aria-pressed', 'true');
  expect(await display.rootTheme()).toBe('dark');
  await page.waitForTimeout(800);
  await shot(page, FOLDER, 'preview-dark', CONTENT);

  await display.save();
  await expect(display.savedToast).toBeVisible({ timeout: 30000 });
  await shot(page, FOLDER, 'saved');

  // After a reload the saved choice overrides the light device.
  await page.reload();
  await page.waitForLoadState('networkidle');
  await expect(display.themeOption('Dark')).toHaveAttribute('aria-pressed', 'true');
  expect(await display.rootTheme()).toBe('dark');
  await shot(page, FOLDER, 'reloaded', CONTENT);
});
