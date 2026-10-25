import { Page, expect, test } from '@playwright/test';
import { InviteAcceptPage, InvitationsPage, SignInPage } from '../e2e/pages';
import { latestTestEmail } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '006-authentication-manage-pending-invitations';

async function zoomOut(page: Page): Promise<void> {
  await page.evaluate(() => {
    document.documentElement.style.zoom = '0.7';
    window.scrollTo(0, 0);
  });
  await page.mouse.move(0, 0);
  await page.waitForTimeout(300);
}

/** Clip from the top of the page content to the bottom of the invitations table card. */
async function contentClip(page: Page) {
  await page.setViewportSize({ width: 1280, height: 800 });
  const layout = await page.locator('.invite-layout').boundingBox();
  const head = await page.locator('.ph-page__head').boundingBox();
  expect(layout && head).toBeTruthy();
  const x = 300;
  const y = head!.y - 8;
  return { x, y, width: 1160 - x, height: layout!.y + layout!.height + 16 - y };
}

test('capture: manage pending invitations', async ({ page, request, browser }) => {
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

  await page.setViewportSize({ width: 1280, height: 800 });
  await new SignInPage(page).signIn({ email: patEmail, password: 'Password123' });
  const invitations = new InvitationsPage(page);
  await invitations.goto();
  await expect(invitations.sendButton).toBeVisible();

  const email = `david-${crypto.randomUUID().slice(0, 8)}@waypoint.demo`;
  await invitations.emails.fill(email);
  await invitations.sendButton.click();
  await expect(page.getByText('Invitation sent.', { exact: true })).toBeVisible();
  const firstToken = /token=([^\s]+)/.exec((await latestTestEmail(request, email)).body)?.[1];
  expect(firstToken).toBeTruthy();

  const search = async () => {
    await page.getByLabel('Search invitations').fill(email);
    await page.getByRole('button', { name: 'Apply search' }).click();
    await expect(invitations.rows).toHaveCount(1);
  };
  await search();
  const row = invitations.rows.filter({ hasText: email });
  await expect(row.getByText('Pending')).toBeVisible();
  await zoomOut(page);
  await shot(page, FOLDER, 'pending', await contentClip(page));

  // Resend: a fresh link is issued.
  await page.evaluate(() => (document.documentElement.style.zoom = ''));
  await row.getByRole('button', { name: 'Resend' }).click();
  await expect(page.getByText(`A fresh invitation was sent to ${email}.`)).toBeVisible();
  const secondToken = /token=([^\s]+)/.exec((await latestTestEmail(request, email)).body)?.[1];
  expect(secondToken).toBeTruthy();
  expect(secondToken).not.toBe(firstToken);
  await zoomOut(page);
  await shot(page, FOLDER, 'resent', await contentClip(page));

  // Revoke asks for confirmation first.
  await page.evaluate(() => (document.documentElement.style.zoom = ''));
  await row.getByRole('button', { name: 'Revoke' }).click();
  await expect(row.getByText('Revoke now?')).toBeVisible();
  await zoomOut(page);
  await shot(page, FOLDER, 'confirm', await contentClip(page));

  await page.evaluate(() => (document.documentElement.style.zoom = ''));
  await row.getByRole('button', { name: 'Confirm' }).click();
  await expect(page.getByText(`The invitation for ${email} was revoked.`)).toBeVisible();
  await page.getByLabel('Status').first().selectOption('Revoked');
  await expect(row.getByText('Revoked', { exact: true })).toBeVisible();
  await zoomOut(page);
  await shot(page, FOLDER, 'revoked', await contentClip(page));

  // The link that was emailed no longer works.
  const stale = await browser.newPage({ baseURL: 'http://localhost:4200' });
  await new InviteAcceptPage(stale).goto(secondToken!);
  await expect(stale.getByRole('heading', { name: /no longer active/i })).toBeVisible();
  await stale.setViewportSize({ width: 1280, height: 800 });
  const box = await stale.locator('.ph-authcard').boundingBox();
  await shot(stale, FOLDER, 'dead-link', {
    x: box!.x - 24,
    y: Math.max(0, box!.y - 24),
    width: box!.width + 48,
    height: box!.height + 48,
  });
});
