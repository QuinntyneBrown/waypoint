import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';

// Traces to: L2-AUTH-006
test('five failed sign-ins lock the account and show the locked state', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'lockout');

  await page.goto('/auth/sign-in');
  await page.getByLabel('Email').fill(creds.email);
  const password = page.getByLabel('Password', { exact: true });
  const submit = page.getByRole('button', { name: /sign in/i });

  // Attempts 1–4 fail with the generic message and stay on the form.
  for (let i = 0; i < 4; i++) {
    await password.fill('WrongPassword9');
    await submit.click();
    await expect(page.getByRole('alert')).toContainText(/invalid email or password/i);
  }

  // The 5th failure crosses the threshold → 423 → account-locked state.
  await password.fill('WrongPassword9');
  await submit.click();

  await expect(page).toHaveURL(/\/auth\/account-locked/);
  await expect(page.getByRole('heading', { name: /too many sign-in attempts/i })).toBeVisible();
  await expect(page.getByText(/15 minutes/i)).toBeVisible();
  await expect(page.getByRole('link', { name: /reset password/i })).toBeVisible();
  await expect(page.getByRole('link', { name: /contact your mentor/i })).toBeVisible();
});
