import { expect, test } from '@playwright/test';
import { AccountLockedPage, ContactPage, ForgotPasswordPage, SignInPage } from '../e2e/pages';
import { shot } from './shots';

const CARD = { x: 380, y: 40, width: 520, height: 720 };
const LOCKED = { x: 340, y: 240, width: 600, height: 320 };

const FOLDER = '008-authentication-recover-locked-account';

test('capture: recover a locked account', async ({ page, request }) => {
  const email = `imani-${crypto.randomUUID().slice(0, 6)}@waypoint.demo`;
  const seeded = await request.post('/api/_test/users', {
    data: { email, password: 'Password123', displayName: 'Imani Clarke', role: 'Youth' },
  });
  expect(seeded.ok()).toBeTruthy();

  const signIn = new SignInPage(page);
  await signIn.goto();
  await signIn.email.fill(email);
  for (let i = 0; i < 4; i++) {
    await signIn.password.fill('WrongPassword9');
    await signIn.submitButton.click();
    await expect(signIn.errorAlert).toContainText(/invalid email or password/i);
  }
  await shot(page, FOLDER, 'fourth-miss', CARD);

  await signIn.password.fill('WrongPassword9');
  await signIn.submitButton.click();
  const locked = new AccountLockedPage(page);
  await expect(page).toHaveURL(AccountLockedPage.urlPattern);
  await expect(locked.heading).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'locked', LOCKED);

  await locked.resetPasswordLink.click();
  await expect(page).toHaveURL(ForgotPasswordPage.path);
  await expect(new ForgotPasswordPage(page).sendButton).toBeVisible();
  await shot(page, FOLDER, 'reset', CARD);

  await page.goBack();
  await locked.contactMentorLink.click();
  await expect(page).toHaveURL('http://localhost:4201/contact');
  await expect(new ContactPage(page).heading).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'contact');
});
