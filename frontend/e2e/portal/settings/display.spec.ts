import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { DisplaySettingsPage, SignInPage } from '../../pages';

// Traces to: L2-PLATFORM-004
test('display preferences preview live and persist across reloads', async ({ page, request }) => {
  const creds = await registerYouth(request, 'display');
  await new SignInPage(page).signIn(creds);

  const display = new DisplaySettingsPage(page);
  await display.goto();

  // Increasing text size updates the live root token and the readout.
  await display.increaseTextSizeButton.click();
  await expect(display.textScaleReadout).toHaveText('110%');
  expect(await display.textScaleToken()).toBe('1.1');

  // Toggle high contrast (custom switch hides the native input) and save.
  await display.toggleSwitch('High contrast');
  expect(await display.hasContrastClass()).toBe(true);
  await display.save();
  // Wait for the save to complete before reloading (else the PUT is cancelled).
  await expect(display.savedToast).toBeVisible();

  // Reload: the saved preferences load from the server and re-apply app-wide.
  await page.reload();
  await page.waitForLoadState('networkidle');
  await expect(display.textScaleReadout).toHaveText('110%');
  expect(await display.hasContrastClass()).toBe(true);
});
