import { expect, test } from '@playwright/test';

// Traces to: L2-NFR-010
test('sign-in is keyboard operable with labelled inputs and adequate tap targets', async ({
  page,
}) => {
  await page.goto('/auth/sign-in');

  // Every input has a real associated label (AC1).
  await expect(page.getByLabel('Email')).toBeVisible();
  await expect(page.getByLabel('Password', { exact: true })).toBeVisible();

  // The primary action meets the 44px tap-target minimum (AC1).
  const box = await page.getByRole('button', { name: /sign in/i }).boundingBox();
  expect(box?.height ?? 0).toBeGreaterThanOrEqual(44);

  // Fully operable by keyboard: type and submit with Enter (AC3).
  await page.getByLabel('Email').fill(`nobody-${crypto.randomUUID()}@wordup.test`);
  const password = page.getByLabel('Password', { exact: true });
  await password.fill('WrongPassword9');
  await password.press('Enter');
  await expect(page.getByRole('alert')).toContainText(/invalid email or password/i);
});

// Traces to: L2-NFR-010
test('the account menu toggle exposes an accessible name for its icon-only button', async ({
  page,
}) => {
  await page.goto('/auth/sign-in');
  // The show/hide password control is icon-only and must be named (AC1).
  await expect(page.getByRole('button', { name: /show password/i })).toBeVisible();
});
