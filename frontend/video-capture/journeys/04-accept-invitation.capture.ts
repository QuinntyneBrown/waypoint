import { Page, expect, test } from '@playwright/test';
import { InvitationsPage, InviteAcceptPage, OnboardingPage, SignInPage } from '../../e2e/pages';
import { latestTestEmail, signInApi } from '../../e2e/support/api';
import { shot } from '../shots';

const FOLDER = 'journeys/04-accept-invitation';

async function zoomOut(page: Page): Promise<void> {
  await page.evaluate(() => {
    document.documentElement.style.zoom = '0.7';
    window.scrollTo(0, 0);
  });
  await page.waitForTimeout(300);
}

async function cardClip(page: Page) {
  await page.setViewportSize({ width: 1280, height: 800 });
  const box = await page.locator('.w-authcard').boundingBox();
  expect(box).not.toBeNull();
  const pad = 24;
  const y = Math.max(0, box!.y - pad);
  return {
    x: Math.max(0, box!.x - pad),
    y,
    width: box!.width + pad * 2,
    height: Math.min(800 - y, box!.height + pad * 2),
  };
}

async function tokenFor(request: import('@playwright/test').APIRequestContext, email: string) {
  const token = /token=([^\s]+)/.exec((await latestTestEmail(request, email)).body)?.[1];
  expect(token).toBeTruthy();
  return token!;
}

test('capture: journey 04 accept invitation', async ({ page, request, browser }) => {
  const patEmail = `pat-${crypto.randomUUID()}@waypoint.demo`;
  const created = await request.post('/api/_test/users', {
    data: {
      email: patEmail,
      password: 'Password123',
      displayName: 'Pat Morgan',
      role: 'ProgramAdministrator',
    },
  });
  expect(created.ok()).toBeTruthy();

  // Pat sends two invitations through the real screen.
  await page.setViewportSize({ width: 1280, height: 800 });
  await new SignInPage(page).signIn({ email: patEmail, password: 'Password123' });
  const invitations = new InvitationsPage(page);
  await invitations.goto();
  await expect(invitations.sendButton).toBeVisible();
  await page.waitForLoadState('networkidle');

  const amaraEmail = `amara-${crypto.randomUUID().slice(0, 8)}@waypoint.demo`;
  const staleEmail = `revoked-${crypto.randomUUID().slice(0, 8)}@waypoint.demo`;
  await page.getByLabel('Role', { exact: true }).first().selectOption('Youth');
  await invitations.emails.fill(`${amaraEmail}\n${staleEmail}`);
  await zoomOut(page);
  await shot(page, FOLDER, 'issue', { x: 305, y: 55, width: 305, height: 625 });

  await invitations.sendButton.click();
  await expect(page.getByText('2 invitations sent.', { exact: true })).toBeVisible();
  await page.getByLabel('Search invitations').fill(amaraEmail);
  await page.getByRole('button', { name: 'Apply search' }).click();
  await expect(invitations.rows).toHaveCount(1);
  await page.mouse.move(0, 0);
  await zoomOut(page);
  await shot(page, FOLDER, 'sent');

  // Amara opens her personal link in a fresh browser.
  const amara = await browser.newPage({ baseURL: 'http://localhost:4200' });
  const invite = new InviteAcceptPage(amara);
  await invite.goto(await tokenFor(request, amaraEmail));
  await expect(invite.heading).toContainText(/join waypoint as youth/i);
  await amara.waitForLoadState('networkidle');
  await shot(amara, FOLDER, 'invite', await cardClip(amara));

  await invite.displayName.fill('Amara Bell');
  await invite.password.fill('Password123');
  await invite.confirmPassword.fill('Password123');
  await invite.agreeToGuidelines();
  await shot(amara, FOLDER, 'filled', await cardClip(amara));

  await invite.joinButton.click();
  await expect(amara).toHaveURL(OnboardingPage.urlPattern);

  // The same link a second time: single use.
  const reuse = await browser.newPage({ baseURL: 'http://localhost:4200' });
  await new InviteAcceptPage(reuse).goto(await tokenFor(request, amaraEmail));
  await expect(reuse.getByRole('heading', { name: /already been used/i })).toBeVisible();
  await reuse.waitForLoadState('networkidle');
  await shot(reuse, FOLDER, 'used', await cardClip(reuse));

  // Pat revokes the other invitation; its link stops working.
  const staleToken = await tokenFor(request, staleEmail);
  const auth = await signInApi(request, { email: patEmail, password: 'Password123' });
  const headers = { Authorization: `Bearer ${auth.accessToken}` };
  const list = await request.get(`/api/invitations?search=${encodeURIComponent(staleEmail)}`, {
    headers,
  });
  const { items } = (await list.json()) as { items: { id: string; email: string }[] };
  const row = items.find((i) => i.email === staleEmail);
  expect(row).toBeTruthy();
  const revoke = await request.post(`/api/invitations/${row!.id}/revoke`, { headers });
  expect(revoke.ok(), await revoke.text()).toBeTruthy();
  const stale = await browser.newPage({ baseURL: 'http://localhost:4200' });
  await new InviteAcceptPage(stale).goto(staleToken);
  await expect(stale.getByRole('heading', { name: /no longer active/i })).toBeVisible();
  await expect(stale.getByRole('button', { name: /request a new invite/i })).toBeVisible();
  await stale.waitForLoadState('networkidle');
  await shot(stale, FOLDER, 'revoked', await cardClip(stale));
});
