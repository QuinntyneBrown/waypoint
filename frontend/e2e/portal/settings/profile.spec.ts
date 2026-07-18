import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { signIn } from '../../support/sign-in';

// Traces to: L2-PLATFORM-002
test('a user updates their profile and sees a confirmation', async ({ page, request }) => {
  const creds = await registerYouth(request, 'profile');
  await signIn(page, creds);
  await page.goto('/settings/profile');

  await page.getByLabel('Display name').fill('Sam the Leader');
  await page.getByLabel('Pronouns').fill('they/them');
  await page.getByRole('button', { name: /save profile/i }).click();

  await expect(page.getByText(/profile updated/i)).toBeVisible();
});

// Traces to: L2-PLATFORM-002
test('change-password enforces policy and blocks mismatches client-side', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'changepw');
  await signIn(page, creds);
  await page.goto('/settings/profile');

  const change = page.getByRole('button', { name: /change password/i });

  await page.getByLabel('Current password').fill(creds.password);
  await page.getByLabel('New password', { exact: true }).fill('short');
  await page.getByLabel('Confirm new password', { exact: true }).fill('short');
  await expect(page.getByText(/at least 8 characters/i)).toBeVisible();
  await expect(change).toBeDisabled();

  await page.getByLabel('New password', { exact: true }).fill('NewPassword9');
  await page.getByLabel('Confirm new password', { exact: true }).fill('Mismatch9');
  await expect(page.getByText(/do not match/i)).toBeVisible();
  await expect(change).toBeDisabled();

  await page.getByLabel('Confirm new password', { exact: true }).fill('NewPassword9');
  await expect(change).toBeEnabled();
});
