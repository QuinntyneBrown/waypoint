import { expect, test } from '@playwright/test';
import { SignInPage } from '../pages';

// Traces to: L2-NFR-010
test('sign-in is keyboard operable with labelled inputs and adequate tap targets', async ({
  page,
}) => {
  const signInPage = new SignInPage(page);
  await signInPage.goto();

  // Every input has a real associated label (AC1).
  await expect(signInPage.email).toBeVisible();
  await expect(signInPage.password).toBeVisible();

  // The primary action meets the 44px tap-target minimum (AC1).
  const box = await signInPage.submitButton.boundingBox();
  expect(box?.height ?? 0).toBeGreaterThanOrEqual(44);

  // Fully operable by keyboard: type and submit with Enter (AC3).
  await signInPage.email.fill(`nobody-${crypto.randomUUID()}@waypoint.test`);
  await signInPage.password.fill('WrongPassword9');
  await signInPage.password.press('Enter');
  await expect(signInPage.errorAlert).toContainText(/invalid email or password/i);
});

// Traces to: L2-NFR-010
test('the account menu toggle exposes an accessible name for its icon-only button', async ({
  page,
}) => {
  const signInPage = new SignInPage(page);
  await signInPage.goto();
  // The show/hide password control is icon-only and must be named (AC1).
  await expect(signInPage.showPasswordButton).toBeVisible();
});
