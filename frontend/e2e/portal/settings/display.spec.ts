import { expect, Page, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { signIn } from '../../support/sign-in';

const toggleSwitch = (page: Page, label: string) =>
  page.locator('label.wu-switch', { has: page.locator(`[aria-label="${label}"]`) }).click();

// Traces to: L2-PLATFORM-004
test('display preferences preview live and persist across reloads', async ({ page, request }) => {
  const creds = await registerYouth(request, 'display');
  await signIn(page, creds);
  await page.goto('/settings/display');

  // Increasing text size updates the live root token and the readout.
  await page.getByRole('button', { name: 'Increase text size' }).click();
  await expect(page.getByTestId('text-scale')).toHaveText('110%');
  expect(
    await page.evaluate(() => document.documentElement.style.getPropertyValue('--wu-text-scale')),
  ).toBe('1.1');

  // Toggle high contrast (custom switch hides the native input) and save.
  await toggleSwitch(page, 'High contrast');
  expect(
    await page.evaluate(() => document.documentElement.classList.contains('wu-a11y-contrast')),
  ).toBe(true);
  await page.getByRole('button', { name: /save settings/i }).click();
  // Wait for the save to complete before reloading (else the PUT is cancelled).
  await expect(page.getByText(/display settings saved/i)).toBeVisible();

  // Reload: the saved preferences load from the server and re-apply app-wide.
  await page.reload();
  await page.waitForLoadState('networkidle');
  await expect(page.getByTestId('text-scale')).toHaveText('110%');
  expect(
    await page.evaluate(() => document.documentElement.classList.contains('wu-a11y-contrast')),
  ).toBe(true);
});
