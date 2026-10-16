import { expect, test } from '@playwright/test';
import {
  ForgotPasswordPage,
  ResetLinkInvalidPage,
  ResetLinkSentPage,
  ResetPasswordPage,
  SignInPage,
  YouthHomePage,
} from '../e2e/pages';
import { latestTestEmail } from '../e2e/support/api';
import { shot } from './shots';

const CARD = { x: 380, y: 40, width: 520, height: 720 };
const STATE = { x: 290, y: 150, width: 700, height: 500 };

const FOLDER = '009-authentication-recover-password';

test('capture: recover a password', async ({ page, request }) => {
  const email = `amara-${crypto.randomUUID().slice(0, 6)}@waypoint.demo`;
  const oldPassword = 'Password123';
  const newPassword = 'Journey2026';
  const seeded = await request.post('/api/_test/users', {
    data: { email, password: oldPassword, displayName: 'Amara Bell', role: 'Youth' },
  });
  expect(seeded.ok()).toBeTruthy();

  const forgot = new ForgotPasswordPage(page);
  await forgot.goto();
  await forgot.email.fill(email);
  await shot(page, FOLDER, 'forgot', CARD);

  await forgot.sendButton.click();
  const sent = new ResetLinkSentPage(page);
  await expect(page).toHaveURL(ResetLinkSentPage.urlPattern);
  await expect(sent.maskedEmail(email)).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'sent', STATE);

  const message = await latestTestEmail(request, email);
  const token = /token=([^\s]+)/.exec(message.body)?.[1];
  expect(token).toBeTruthy();

  const reset = new ResetPasswordPage(page);
  await reset.goto(token!);
  await reset.newPassword.fill(newPassword);
  await reset.confirmPassword.fill(newPassword);
  await expect(reset.submitButton).toBeEnabled();
  await shot(page, FOLDER, 'reset', CARD);

  await reset.submitButton.click();
  await expect(page).toHaveURL(/\/auth\/reset-success/);
  await expect(page.getByRole('heading', { name: /your password was updated/i })).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'success', STATE);

  // The same link cannot be used twice.
  await reset.goto(token!);
  await reset.submit('Another2026');
  await expect(page).toHaveURL(ResetLinkInvalidPage.urlPattern);
  await expect(new ResetLinkInvalidPage(page).heading).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'invalid', STATE);

  await new SignInPage(page).signIn({ email, password: newPassword });
  await expect(page).not.toHaveURL(/\/auth\/sign-in/);
  void YouthHomePage;
});
