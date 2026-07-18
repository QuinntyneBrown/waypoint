import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { AccountLockedPage, ContactPage, SignInPage } from '../../pages';

// Traces to: L2-AUTH-006
test('five failed sign-ins lock the account and show the locked state', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'lockout');

  const signInPage = new SignInPage(page);
  await signInPage.goto();
  await signInPage.email.fill(creds.email);

  // Attempts 1–4 fail with the generic message and stay on the form.
  for (let i = 0; i < 4; i++) {
    await signInPage.password.fill('WrongPassword9');
    await signInPage.submitButton.click();
    await expect(signInPage.errorAlert).toContainText(/invalid email or password/i);
  }

  // The 5th failure crosses the threshold → 423 → account-locked state.
  await signInPage.password.fill('WrongPassword9');
  await signInPage.submitButton.click();

  const locked = new AccountLockedPage(page);
  await expect(page).toHaveURL(AccountLockedPage.urlPattern);
  await expect(locked.heading).toBeVisible();
  await expect(locked.lockoutDurationText).toBeVisible();
  await expect(locked.resetPasswordLink).toBeVisible();
  await expect(locked.contactMentorLink).toBeVisible();

  await locked.contactMentorLink.click();
  await expect(page).toHaveURL('http://localhost:4201/contact');
  await expect(new ContactPage(page).heading).toBeVisible();
});
