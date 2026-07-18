import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { SessionExpiredPage, SignInPage, YouthHomePage } from '../../pages';

// Traces to: L2-AUTH-012
test('refresh-token reuse revokes the family and forces the session-expired state', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'reuse');
  await new SignInPage(page).signIn(creds);
  await expect(page).toHaveURL(YouthHomePage.urlPattern);

  // Capture the app's stored refresh token and seed an in-progress draft.
  const session = await page.evaluate(() => sessionStorage.getItem('wu.session.v1'));
  const { refreshToken, user } = JSON.parse(session!);
  const draftKey = `wu.draft.${user.userId}.reflection`;
  await page.evaluate((k) => localStorage.setItem(k, 'my unsaved reflection'), draftKey);

  // Present the captured refresh token twice: the second use trips reuse
  // detection and revokes the whole token family (L2-AUTH-012 AC2).
  const first = await request.post('/api/auth/refresh', { data: { refreshToken } });
  expect(first.ok()).toBeTruthy();
  const second = await request.post('/api/auth/refresh', { data: { refreshToken } });
  expect(second.status()).toBe(401);

  // Reload: the app's boot refresh now 401s → forced teardown to session-expired.
  await page.reload();
  const sessionExpired = new SessionExpiredPage(page);
  await expect(page).toHaveURL(SessionExpiredPage.urlPattern);
  await expect(sessionExpired.heading).toBeVisible();
  await expect(sessionExpired.signInAgainLink).toBeVisible();

  // The draft survives the forced teardown (L2-PLATFORM-010 AC1 / L2-AUTH-007 AC2).
  const draft = await page.evaluate((k) => localStorage.getItem(k), draftKey);
  expect(draft).toBe('my unsaved reflection');
});
