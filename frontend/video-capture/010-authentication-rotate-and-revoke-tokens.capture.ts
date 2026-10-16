import { expect, test } from '@playwright/test';
import { SessionExpiredPage, SignInPage, YouthHomePage } from '../e2e/pages';
import { shot } from './shots';

const STATE = { x: 290, y: 150, width: 700, height: 500 };

const FOLDER = '010-authentication-rotate-and-revoke-tokens';

test('capture: rotate and revoke tokens', async ({ page, request }) => {
  const email = `amara-${crypto.randomUUID().slice(0, 6)}@waypoint.demo`;
  const seeded = await request.post('/api/_test/users', {
    data: { email, password: 'Password123', displayName: 'Amara Bell', role: 'Youth' },
  });
  expect(seeded.ok()).toBeTruthy();

  await new SignInPage(page).signIn({ email, password: 'Password123' });
  const home = new YouthHomePage(page);
  await expect(page).toHaveURL(YouthHomePage.urlPattern);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'signed-in');

  // Present one refresh token twice, as a stolen copy would, to trip reuse detection.
  const session = await page.evaluate(() => sessionStorage.getItem('w.session.v1'));
  const { refreshToken } = JSON.parse(session!);
  const first = await request.post('/api/auth/refresh', { data: { refreshToken } });
  expect(first.ok()).toBeTruthy();
  const second = await request.post('/api/auth/refresh', { data: { refreshToken } });
  expect(second.status()).toBe(401);

  await page.reload();
  const expired = new SessionExpiredPage(page);
  await expect(page).toHaveURL(SessionExpiredPage.urlPattern);
  await expect(expired.heading).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'session-expired', STATE);

  // Signing out from a shared computer ends the session for good.
  await expired.signInAgainLink.click();
  await new SignInPage(page).submitCredentials(email, 'Password123');
  await expect(page).toHaveURL(YouthHomePage.urlPattern);
  await page.waitForLoadState('networkidle');
  const revoked = page.waitForResponse(
    (r) => r.url().includes('/api/auth/sign-out') && r.status() === 204,
  );
  await home.shell.signOut();
  await revoked;
  await expect(page).toHaveURL(/\/auth\/sign-in/);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'signed-out', { x: 380, y: 40, width: 520, height: 720 });
});
