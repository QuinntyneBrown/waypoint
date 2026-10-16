import { expect, test } from '@playwright/test';
import { CohortsPage, DisplaySettingsPage, SignInPage } from '../e2e/pages';
import { signInSlow } from './non-functional-helpers';
import { shot } from './shots';

const FOLDER = '105-non-functional-meet-accessibility-standard';

test.setTimeout(240_000);

test('capture: keyboard focus, display preferences and dialog focus', async ({ page, request }) => {
  const signIn = new SignInPage(page);
  await signIn.goto();
  await expect(signIn.submitButton).toBeVisible();
  await page.waitForLoadState('networkidle');
  await page.keyboard.press('Tab');
  await expect(signIn.email).toBeFocused();
  await shot(page, FOLDER, 'focus-email', { x: 380, y: 90, width: 520, height: 620 });
  await page.keyboard.press('Tab');
  await page.keyboard.press('Tab');
  await page.keyboard.press('Tab');
  await page.keyboard.press('Tab');
  await shot(page, FOLDER, 'focus-later', { x: 380, y: 90, width: 520, height: 620 });

  const email = `pat-${crypto.randomUUID()}@waypoint.demo`;
  const made = await request.post('/api/_test/users', {
    data: {
      email,
      password: 'Password123',
      displayName: 'Pat Morgan',
      role: 'ProgramAdministrator',
    },
  });
  expect(made.ok()).toBeTruthy();
  await signInSlow(page, { email, password: 'Password123' });

  const display = new DisplaySettingsPage(page);
  await display.goto();
  await expect(display.increaseTextSizeButton).toBeVisible();
  await page.waitForLoadState('networkidle');
  await page.setViewportSize({ width: 1280, height: 800 });
  await display.increaseTextSizeButton.click();
  await display.toggleSwitch('High contrast');
  await expect(display.textScaleReadout).toHaveText('110%');
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'display-high', { x: 380, y: 80, width: 780, height: 720 });

  const cohorts = new CohortsPage(page);
  await cohorts.goto();
  await cohorts.createButton.click();
  await expect(cohorts.createDialog.getByRole('button', { name: 'Close' })).toBeFocused();
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'dialog', { x: 280, y: 120, width: 720, height: 560 });
});
