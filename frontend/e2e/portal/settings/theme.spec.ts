import { expect, test } from '@playwright/test';
import { SignInPage } from '../../pages';

const DARK_CANVAS = 'rgb(14, 14, 8)';
const LIGHT_CANVAS = 'rgb(255, 255, 255)';

// Traces to: L2-PLATFORM-013 AC1/AC5
test('signed-out pages follow a dark device colour scheme', async ({ page }) => {
  await page.emulateMedia({ colorScheme: 'dark' });
  const signIn = new SignInPage(page);
  await signIn.goto();

  expect(await signIn.canvasBackground()).toBe(DARK_CANVAS);
  expect(await signIn.colorScheme()).toBe('dark');
});

// Traces to: L2-PLATFORM-013 AC1/AC5
test('signed-out pages follow a light device colour scheme', async ({ page }) => {
  await page.emulateMedia({ colorScheme: 'light' });
  const signIn = new SignInPage(page);
  await signIn.goto();

  expect(await signIn.canvasBackground()).toBe(LIGHT_CANVAS);
  expect(await signIn.colorScheme()).toBe('light');
});
