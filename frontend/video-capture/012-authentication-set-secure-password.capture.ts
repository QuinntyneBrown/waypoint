import { expect, test } from '@playwright/test';
import { InviteAcceptPage, ResetPasswordPage } from '../e2e/pages';
import { latestTestEmail, registerUser, signInApi } from '../e2e/support/api';
import { shot } from './shots';

const CARD = { x: 380, y: 60, width: 520, height: 680 };

const FOLDER = '012-authentication-set-secure-password';

test('capture: set a secure password', async ({ page, request }) => {
  const reset = new ResetPasswordPage(page);
  await reset.goto('capture-token');

  await reset.newPassword.fill('short');
  await reset.confirmPassword.fill('short');
  await expect(reset.policyHint).toBeVisible();
  await shot(page, FOLDER, 'too-short', CARD);

  await reset.newPassword.fill('Journey2026');
  await reset.confirmPassword.fill('Journey2027');
  await expect(reset.mismatchError).toBeVisible();
  await shot(page, FOLDER, 'mismatch', CARD);

  await reset.confirmPassword.fill('Journey2026');
  await expect(reset.submitButton).toBeEnabled();
  await reset.showPasswordButton.first().click();
  await expect(reset.newPassword).toHaveAttribute('type', 'text');
  await shot(page, FOLDER, 'shown', CARD);

  // The same policy applies when an invitee creates a password.
  const admin = await registerUser(request, 'ProgramAdministrator', 'video-invite');
  const auth = await signInApi(request, admin);
  const email = `david-${crypto.randomUUID().slice(0, 6)}@waypoint.demo`;
  const issued = await request.post('/api/invitations', {
    headers: { Authorization: `Bearer ${auth.accessToken}` },
    data: { email, role: 'Youth', cohortId: null, mentorId: null },
  });
  expect(issued.ok(), await issued.text()).toBeTruthy();
  const token = /token=([^\s]+)/.exec((await latestTestEmail(request, email)).body)?.[1];
  expect(token).toBeTruthy();

  const invite = new InviteAcceptPage(page);
  await invite.goto(token!);
  await expect(invite.heading).toContainText(/join waypoint as youth/i);
  await invite.displayName.fill('David Osei');
  await invite.password.fill('short');
  await invite.confirmPassword.fill('short');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'invite', { x: 340, y: 20, width: 600, height: 760 });
});
