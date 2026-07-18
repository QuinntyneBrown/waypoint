import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { SignInPage, YouthHomePage } from '../../pages';

// Traces to: L2-AUTH-001
test('a registered youth signs in and is routed into the youth area', async ({ page, request }) => {
  const creds = await registerYouth(request, 'signin');

  const signInPage = new SignInPage(page);
  await signInPage.goto();
  await signInPage.submitCredentials(creds.email, creds.password);

  const home = new YouthHomePage(page);
  await expect(page).toHaveURL(YouthHomePage.urlPattern);
  await expect(home.welcomeHeading).toBeVisible();
});

// Traces to: L2-AUTH-001
test('sign-in with a wrong password fails with enumeration-safe copy', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'wrongpw');

  const signInPage = new SignInPage(page);
  await signInPage.goto();
  await signInPage.submitCredentials(creds.email, 'WrongPassword9');

  // Same generic message for wrong password and unknown email (no enumeration).
  await expect(signInPage.errorAlert).toContainText(/invalid email or password/i);
  await expect(page).toHaveURL(/\/auth\/sign-in/);
});

// Traces to: L2-AUTH-001
test('sign-in with an unknown email shows the same generic failure', async ({ page }) => {
  const signInPage = new SignInPage(page);
  await signInPage.goto();
  await signInPage.submitCredentials(`nobody-${crypto.randomUUID()}@wordup.test`, 'WrongPassword9');

  await expect(signInPage.errorAlert).toContainText(/invalid email or password/i);
});
