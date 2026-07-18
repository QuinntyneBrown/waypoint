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

  // Sign out from the shell (sidenav foot control).
  await home.shell.signOut();
  await expect(page).toHaveURL(/\/auth\/sign-in/);

  // No account data remains: the session storage is purged and a protected page
  // redirects straight back to sign-in with no silent restore.
  const stored = await page.evaluate(
    () => sessionStorage.getItem('wu.session.v1') ?? localStorage.getItem('wu.session.v1'),
  );
  expect(stored).toBeNull();

  await home.goto();
  await expect(page).toHaveURL(/\/auth\/sign-in/);
});
