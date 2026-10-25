import { expect, test } from '@playwright/test';
import { InviteAcceptPage, OnboardingPage } from '../e2e/pages';
import { latestTestEmail, registerUser, signInApi } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '002-authentication-accept-invitation';

async function issueInvite(
  request: import('@playwright/test').APIRequestContext,
  accessToken: string,
  email: string,
): Promise<string> {
  const issued = await request.post('/api/invitations', {
    headers: { Authorization: `Bearer ${accessToken}` },
    data: { email, role: 'Youth', cohortId: null, mentorId: null },
  });
  expect(issued.ok(), await issued.text()).toBeTruthy();
  const message = await latestTestEmail(request, email);
  const token = /token=([^\s]+)/.exec(message.body)?.[1];
  expect(token).toBeTruthy();
  return token!;
}

async function cardClip(page: import('@playwright/test').Page) {
  await page.setViewportSize({ width: 1280, height: 800 });
  const box = await page.locator('.ph-authcard').boundingBox();
  expect(box).not.toBeNull();
  const pad = 24;
  return {
    x: Math.max(0, box!.x - pad),
    y: Math.max(0, box!.y - pad),
    width: box!.width + pad * 2,
    height: Math.min(800 - Math.max(0, box!.y - pad), box!.height + pad * 2),
  };
}

test('capture: accept an invitation', async ({ page, request }) => {
  const admin = await registerUser(request, 'ProgramAdministrator', 'invite-issuer');
  const auth = await signInApi(request, admin);

  const email = `amara-${crypto.randomUUID()}@waypoint.demo`;
  const token = await issueInvite(request, auth.accessToken, email);

  const invite = new InviteAcceptPage(page);
  await invite.goto(token);
  await expect(invite.heading).toContainText(/join waypoint as youth/i);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'invite', await cardClip(page));

  await invite.displayName.fill('Amara Bell');
  await invite.password.fill('Password123');
  await invite.confirmPassword.fill('Password123');
  await invite.agreeToGuidelines();
  await shot(page, FOLDER, 'filled', await cardClip(page));

  await invite.joinButton.click();
  await expect(page).toHaveURL(/\/onboarding/);
  await expect(new OnboardingPage(page).displayName).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'onboarding');

  // The same link a second time: single-use.
  const reuse = await page.context().newPage();
  await new InviteAcceptPage(reuse).goto(token);
  await expect(reuse.getByRole('heading', { name: /already been used/i })).toBeVisible();
  await shot(reuse, FOLDER, 'used', await cardClip(reuse));

  // A revoked link stops working, and offers a way to ask for a new one.
  const revokedEmail = `revoked-${crypto.randomUUID()}@waypoint.demo`;
  const revokedToken = await issueInvite(request, auth.accessToken, revokedEmail);
  const list = await request.get(`/api/invitations?search=${encodeURIComponent(revokedEmail)}`, {
    headers: { Authorization: `Bearer ${auth.accessToken}` },
  });
  const { items } = (await list.json()) as { items: { id: string; email: string }[] };
  const row = items.find((i) => i.email === revokedEmail);
  expect(row).toBeTruthy();
  const revoke = await request.post(`/api/invitations/${row!.id}/revoke`, {
    headers: { Authorization: `Bearer ${auth.accessToken}` },
  });
  expect(revoke.ok(), await revoke.text()).toBeTruthy();
  const stale = await page.context().newPage();
  await new InviteAcceptPage(stale).goto(revokedToken);
  await expect(stale.getByRole('heading', { name: /no longer active/i })).toBeVisible();
  await expect(stale.getByRole('button', { name: /request a new invite/i })).toBeVisible();
  await shot(stale, FOLDER, 'revoked', await cardClip(stale));
});
