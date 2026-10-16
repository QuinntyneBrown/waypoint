import { expect, test } from '@playwright/test';
import { LandingPage } from '../pages';

// Traces to: L2-PLATFORM-013 AC1/AC5
test('the public site follows the device colour scheme', async ({ page }) => {
  const landing = new LandingPage(page);

  await page.emulateMedia({ colorScheme: 'dark' });
  await landing.goto();
  expect(await landing.canvasBackground()).toBe('rgb(14, 14, 8)');
  expect(await landing.colorScheme()).toBe('dark');

  await page.emulateMedia({ colorScheme: 'light' });
  expect(await landing.canvasBackground()).toBe('rgb(255, 255, 255)');
  expect(await landing.colorScheme()).toBe('light');
});
