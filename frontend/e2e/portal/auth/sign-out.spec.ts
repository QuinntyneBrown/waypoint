import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { SignInPage, YouthHomePage } from '../../pages';

// Traces to: L2-AUTH-007
test('sign-out fully tears down the session and blocks protected pages', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'signout');
  const signInPage = new SignInPage(page);
  await signInPage.signIn(creds);

  const home = new YouthHomePage(page);
  await expect(page).toHaveURL(YouthHomePage.urlPattern);
  const session = await page.evaluate(
    () => sessionStorage.getItem('w.session.v1') ?? localStorage.getItem('w.session.v1'),
  );
  const { refreshToken } = JSON.parse(session!) as { refreshToken: string };

  // Sign out from the shell (sidenav foot control).
  const revoked = page.waitForResponse(
    (response) => response.url().includes('/api/auth/sign-out') && response.status() === 204,
  );
  await home.shell.signOut();
  await revoked;
  await expect(page).toHaveURL(/\/auth\/sign-in/);

  // No account data remains: the session storage is purged and a protected page
  // redirects straight back to sign-in with no silent restore.
  const stored = await page.evaluate(
    () => sessionStorage.getItem('w.session.v1') ?? localStorage.getItem('w.session.v1'),
  );
  expect(stored).toBeNull();
  const refresh = await request.post('/api/auth/refresh', { data: { refreshToken } });
  expect(refresh.status()).toBe(401);

  await home.goto();
  await expect(page).toHaveURL(/\/auth\/sign-in/);
});
