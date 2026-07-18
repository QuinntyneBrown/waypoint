import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { signIn } from '../../support/sign-in';

// Traces to: L2-AUTH-007
test('sign-out fully tears down the session and blocks protected pages', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'signout');
  await signIn(page, creds);
  await expect(page).toHaveURL(/\/youth\/home/);

  // Sign out from the shell (sidenav foot control).
  await page.getByRole('button', { name: 'Sign out' }).click();
  await expect(page).toHaveURL(/\/auth\/sign-in/);

  // No account data remains: the session storage is purged and a protected page
  // redirects straight back to sign-in with no silent restore.
  const stored = await page.evaluate(
    () => sessionStorage.getItem('wu.session.v1') ?? localStorage.getItem('wu.session.v1'),
  );
  expect(stored).toBeNull();

  await page.goto('/youth/home');
  await expect(page).toHaveURL(/\/auth\/sign-in/);
});
