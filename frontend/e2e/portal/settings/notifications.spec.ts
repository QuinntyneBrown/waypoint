import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { signIn } from '../../support/sign-in';

// Traces to: L2-PLATFORM-003
test('notification preferences persist a toggled category across reloads', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'notifprefs');
  await signIn(page, creds);
  await page.goto('/settings/notifications');

  const announce = page.getByRole('checkbox', { name: 'Announcements in-app' });
  await expect(announce).not.toBeChecked(); // default off

  // Toggle via the label (custom check hides the native input) and await the save.
  await page
    .locator('label.wu-check', { has: page.locator('[aria-label="Announcements in-app"]') })
    .click();
  await expect(announce).toBeChecked();
  await expect(page.getByText(/notification preferences saved/i)).toBeVisible();

  await page.reload();
  await page.waitForLoadState('networkidle');
  await expect(page.getByRole('checkbox', { name: 'Announcements in-app' })).toBeChecked();
});
