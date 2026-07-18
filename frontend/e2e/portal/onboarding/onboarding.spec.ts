import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { signIn } from '../../support/sign-in';

// Traces to: L2-AUTH-011
test('a new youth completes the onboarding wizard and lands home', async ({ page, request }) => {
  const creds = await registerYouth(request, 'onboard', { onboarded: false });
  await signIn(page, creds);
  await expect(page).toHaveURL(/\/onboarding/);

  // Step 1 — profile, display name prefilled from the account.
  await expect(page.getByLabel('Display name')).toHaveValue(creds.displayName);
  await page.getByLabel('Pronouns').fill('they/them');
  await page.getByRole('button', { name: /next/i }).click();

  // Step 2 — cohort is read-only; a register-created youth is unplaced (no-cohort).
  await expect(page.getByText(/not placed in a cohort yet/i)).toBeVisible();
  await page.getByRole('button', { name: /next/i }).click();

  // Step 3 — notification defaults: lessons on, mentor on, announcements off.
  await expect(page.getByRole('checkbox', { name: 'Lesson reminders' })).toBeChecked();
  await expect(page.getByRole('checkbox', { name: 'Mentor messages' })).toBeChecked();
  await expect(page.getByRole('checkbox', { name: 'Announcements' })).not.toBeChecked();

  await page.getByRole('button', { name: /finish/i }).click();
  await expect(page).toHaveURL(/\/youth\/home/);
});

// Traces to: L2-AUTH-011
test('a new youth can skip onboarding and still land home', async ({ page, request }) => {
  const creds = await registerYouth(request, 'skip', { onboarded: false });
  await signIn(page, creds);
  await expect(page).toHaveURL(/\/onboarding/);

  await page.getByRole('button', { name: /skip for now/i }).click();
  await expect(page).toHaveURL(/\/youth\/home/);
});
