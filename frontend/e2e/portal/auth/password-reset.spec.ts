import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import {
  ForgotPasswordPage,
  ResetLinkInvalidPage,
  ResetLinkSentPage,
  ResetPasswordPage,
} from '../../pages';

// Traces to: L2-AUTH-005
test('forgot-password gives an identical masked confirmation for known and unknown emails', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'reset');

  const forgot = new ForgotPasswordPage(page);
  const sent = new ResetLinkSentPage(page);

  // Known email → confirmation with masked address + resend.
  await forgot.requestReset(creds.email);
  await expect(page).toHaveURL(ResetLinkSentPage.urlPattern);
  await expect(sent.maskedEmail(creds.email)).toBeVisible();
  await expect(sent.exactEmail(creds.email)).toHaveCount(0);
  await expect(sent.resendButton).toBeVisible();

  // Unknown email → identical confirmation UI (no enumeration).
  await forgot.requestReset(`nobody-${crypto.randomUUID()}@waypoint.test`);
  await expect(page).toHaveURL(ResetLinkSentPage.urlPattern);
  await expect(sent.resendButton).toBeVisible();
});

// Traces to: L2-AUTH-008
test('reset-password enforces policy, blocks mismatches, and toggles visibility', async ({
  page,
}) => {
  const reset = new ResetPasswordPage(page);
  await reset.goto('dummy-token');

  // Weak password → policy feedback, submit blocked.
  await reset.newPassword.fill('short');
  await reset.confirmPassword.fill('short');
  await expect(reset.policyHint).toBeVisible();
  await expect(reset.submitButton).toBeDisabled();

  // Compliant but mismatched → mismatch error, submit blocked.
  await reset.newPassword.fill('Password123');
  await reset.confirmPassword.fill('Password999');
  await expect(reset.mismatchError).toBeVisible();
  await expect(reset.submitButton).toBeDisabled();

  // Compliant + matching → submit enabled.
  await reset.confirmPassword.fill('Password123');
  await expect(reset.submitButton).toBeEnabled();

  // Show/hide toggle flips the field type.
  await expect(reset.newPassword).toHaveAttribute('type', 'password');
  await reset.showPasswordButton.first().click();
  await expect(reset.newPassword).toHaveAttribute('type', 'text');
});

// Traces to: L2-AUTH-005
test('a garbage reset token is rejected with the invalid-link state', async ({ page }) => {
  const reset = new ResetPasswordPage(page);
  await reset.goto('not-a-real-token');
  await reset.submit('Password123');

  const invalid = new ResetLinkInvalidPage(page);
  await expect(page).toHaveURL(ResetLinkInvalidPage.urlPattern);
  await expect(invalid.heading).toBeVisible();
});
