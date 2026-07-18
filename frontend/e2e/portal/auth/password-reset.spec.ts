import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';

// Traces to: L2-AUTH-005
test('forgot-password gives an identical masked confirmation for known and unknown emails', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'reset');

  // Known email → confirmation with masked address + resend.
  await page.goto('/auth/forgot-password');
  await page.getByLabel('Email').fill(creds.email);
  await page.getByRole('button', { name: /send reset link/i }).click();
  await expect(page).toHaveURL(/\/auth\/reset-link-sent/);
  const masked = `${creds.email[0]}•••@${creds.email.split('@')[1]}`;
  await expect(page.getByText(masked)).toBeVisible();
  await expect(page.getByText(creds.email, { exact: true })).toHaveCount(0);
  await expect(page.getByRole('button', { name: /resend/i })).toBeVisible();

  // Unknown email → identical confirmation UI (no enumeration).
  await page.goto('/auth/forgot-password');
  await page.getByLabel('Email').fill(`nobody-${crypto.randomUUID()}@wordup.test`);
  await page.getByRole('button', { name: /send reset link/i }).click();
  await expect(page).toHaveURL(/\/auth\/reset-link-sent/);
  await expect(page.getByRole('button', { name: /resend/i })).toBeVisible();
});

// Traces to: L2-AUTH-008
test('reset-password enforces policy, blocks mismatches, and toggles visibility', async ({
  page,
}) => {
  await page.goto('/auth/reset-password?token=dummy-token');
  const pw = page.getByLabel('New password', { exact: true });
  const confirm = page.getByLabel('Confirm password', { exact: true });
  const submit = page.getByRole('button', { name: /reset password/i });

  // Weak password → policy feedback, submit blocked.
  await pw.fill('short');
  await confirm.fill('short');
  await expect(page.getByText(/at least 8 characters/i)).toBeVisible();
  await expect(submit).toBeDisabled();

  // Compliant but mismatched → mismatch error, submit blocked.
  await pw.fill('Password123');
  await confirm.fill('Password999');
  await expect(page.getByText(/do not match/i)).toBeVisible();
  await expect(submit).toBeDisabled();

  // Compliant + matching → submit enabled.
  await confirm.fill('Password123');
  await expect(submit).toBeEnabled();

  // Show/hide toggle flips the field type.
  await expect(pw).toHaveAttribute('type', 'password');
  await page
    .getByRole('button', { name: /show password/i })
    .first()
    .click();
  await expect(pw).toHaveAttribute('type', 'text');
});

// Traces to: L2-AUTH-005
test('a garbage reset token is rejected with the invalid-link state', async ({ page }) => {
  await page.goto('/auth/reset-password?token=not-a-real-token');
  await page.getByLabel('New password', { exact: true }).fill('Password123');
  await page.getByLabel('Confirm password', { exact: true }).fill('Password123');
  await page.getByRole('button', { name: /reset password/i }).click();

  await expect(page).toHaveURL(/\/auth\/reset-link-invalid/);
  await expect(page.getByRole('heading', { name: /(invalid|expired)/i })).toBeVisible();
});
