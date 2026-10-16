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

const FOLDER = '084-platform-services-configure-display';
const CONTENT = { x: 244, y: 80, width: 1036, height: 720 };

test('capture: configure display', async ({ page, request }) => {
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

  await signInSlow(page, email, password);
  const display = new DisplaySettingsPage(page);
  await display.goto();
  await expect(display.increaseTextSizeButton).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'default', CONTENT);

  await display.increaseTextSizeButton.click();
  await display.increaseTextSizeButton.click();
  await expect(display.textScaleReadout).toHaveText('120%');
  await display.toggleSwitch('High contrast');
  await display.toggleSwitch('Underline links');
  expect(await display.hasContrastClass()).toBe(true);
  await shot(page, FOLDER, 'preview', CONTENT);

  await display.save();
  await expect(display.savedToast).toBeVisible({ timeout: 30000 });
  await shot(page, FOLDER, 'saved');

  await page.reload();
  await page.waitForLoadState('networkidle');
  await expect(display.textScaleReadout).toHaveText('120%');
  await shot(page, FOLDER, 'reloaded', CONTENT);
});
