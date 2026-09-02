import { expect, test } from '@playwright/test';
import { InviteAcceptPage } from '../../pages';
import { latestTestEmail, registerUser, signInApi } from '../../support/api';

// Traces to: L2-AUTH-004, L2-AUTH-008
test('an invited youth accepts a contextual invite and continues to onboarding', async ({
  page,
  request,
}) => {
  const admin = await registerUser(request, 'ProgramAdministrator', 'invite-issuer');
  const auth = await signInApi(request, admin);
  const email = `accepted-invite-${crypto.randomUUID()}@waypoint.test`;
  const issued = await request.post('/api/invitations', {
    headers: { Authorization: `Bearer ${auth.accessToken}` },
    data: { email, role: 'Youth', cohortId: null, mentorId: null },
  });
  expect(issued.ok(), await issued.text()).toBeTruthy();
  const message = await latestTestEmail(request, email);
  const token = /token=([^\s]+)/.exec(message.body)?.[1];
  expect(token).toBeTruthy();

  const invite = new InviteAcceptPage(page);
  await invite.goto(token!);
  await expect(invite.heading).toContainText(/join waypoint as youth/i);
  await invite.displayName.fill('Invited Youth');
  await invite.password.fill('Password123');
  await invite.confirmPassword.fill('Password123');
  await invite.agreeToGuidelines();
  await invite.joinButton.click();

  await expect(page).toHaveURL(/\/onboarding/);
});
