import { expect, test } from '@playwright/test';
import { InviteAcceptPage } from '../e2e/pages';
import { latestTestEmail, signInApi } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '094-platform-services-replace-expired-invitation';
const CARD = { x: 380, y: 90, width: 520, height: 620 };

test('capture: expired invitation', async ({ page, request }) => {
  const adminEmail = `pat-${crypto.randomUUID()}@waypoint.demo`;
  const seeded = await request.post('/api/_test/users', {
    data: {
      email: adminEmail,
      password: 'Password123',
      displayName: 'Pat Morgan',
      role: 'ProgramAdministrator',
    },
  });
  expect(seeded.ok()).toBeTruthy();
  const admin = await signInApi(request, { email: adminEmail, password: 'Password123' });
  const inviteeEmail = `amara-${crypto.randomUUID()}@waypoint.demo`;
  const issued = await request.post('/api/invitations', {
    headers: { Authorization: `Bearer ${admin.accessToken}` },
    data: { email: inviteeEmail, role: 'Youth', cohortId: null, mentorId: null },
  });
  expect(issued.ok(), await issued.text()).toBeTruthy();
  const message = await latestTestEmail(request, inviteeEmail);
  const token = /token=([^\s]+)/.exec(message.body)?.[1];
  expect(token).toBeTruthy();

  // The link is real; only its state is shown as expired, because the lifetime is days long.
  await page.route('**/api/invitations/context**', async (route) => {
    const response = await route.fetch();
    const body = await response.json();
    await route.fulfill({ response, json: { ...body, state: 'expired' } });
  });
  await new InviteAcceptPage(page).goto(token!);
  await expect(page.getByRole('heading', { name: /invite link has expired/i })).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'expired', CARD);

  await page.getByRole('button', { name: /request a new invite/i }).click();
  await expect(page.getByRole('status')).toContainText(/has been notified/i);
  await shot(page, FOLDER, 'requested', CARD);
});
