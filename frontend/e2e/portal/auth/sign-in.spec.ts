import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';

// Traces to: L2-AUTH-001
test('a registered youth signs in and is routed into the youth area', async ({ page, request }) => {
  const creds = await registerYouth(request, 'signin');

  await page.goto('/auth/sign-in');
  await page.getByLabel('Email').fill(creds.email);
  await page.getByLabel('Password', { exact: true }).fill(creds.password);
  await page.getByRole('button', { name: /sign in/i }).click();

  await expect(page).toHaveURL(/\/youth\/home/);
  await expect(page.getByRole('heading', { name: /welcome/i })).toBeVisible();
});

// Traces to: L2-AUTH-001
test('sign-in with a wrong password fails with enumeration-safe copy', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'wrongpw');

  await page.goto('/auth/sign-in');
  await page.getByLabel('Email').fill(creds.email);
  await page.getByLabel('Password', { exact: true }).fill('WrongPassword9');
  await page.getByRole('button', { name: /sign in/i }).click();

  // Same generic message for wrong password and unknown email (no enumeration).
  const alert = page.getByRole('alert');
  await expect(alert).toContainText(/invalid email or password/i);
  await expect(page).toHaveURL(/\/auth\/sign-in/);
});

// Traces to: L2-AUTH-001
test('sign-in with an unknown email shows the same generic failure', async ({ page }) => {
  await page.goto('/auth/sign-in');
  await page.getByLabel('Email').fill(`nobody-${crypto.randomUUID()}@wordup.test`);
  await page.getByLabel('Password', { exact: true }).fill('WrongPassword9');
  await page.getByRole('button', { name: /sign in/i }).click();

  await expect(page.getByRole('alert')).toContainText(/invalid email or password/i);
});
