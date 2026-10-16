import { expect, test } from '@playwright/test';
import {
  ForgotPasswordPage,
  ResetLinkInvalidPage,
  ResetLinkSentPage,
  ResetPasswordPage,
  SignInPage,
  YouthHomePage,
} from '../../e2e/pages';
import { latestTestEmail } from '../../e2e/support/api';
import { shot } from '../shots';

const CARD = { x: 380, y: 40, width: 520, height: 720 };
const SIGN_IN = { x: 380, y: 90, width: 520, height: 620 };
const STATE = { x: 290, y: 150, width: 700, height: 500 };

const FOLDER = 'journeys/24-reset-password';

test('capture: journey reset password', async ({ page, request }) => {
  const email = `amara-${crypto.randomUUID().slice(0, 6)}@waypoint.demo`;
  const oldPassword = 'Password123';
  const newPassword = 'Journey2026';
  const seeded = await request.post('/api/_test/users', {
    data: { email, password: oldPassword, displayName: 'Amara Bell', role: 'Youth' },
  });
  expect(seeded.ok()).toBeTruthy();

  const signIn = new SignInPage(page);
  await signIn.goto();
  await expect(signIn.submitButton).toBeVisible();
  await shot(page, FOLDER, 'sign-in', SIGN_IN);

  await page.getByRole('link', { name: /forgot password/i }).click();
  const forgot = new ForgotPasswordPage(page);
  await expect(page).toHaveURL(ForgotPasswordPage.path);
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

  await reset.goto(token!);
  await reset.submit('Another2026');
  await expect(page).toHaveURL(ResetLinkInvalidPage.urlPattern);
  await expect(new ResetLinkInvalidPage(page).heading).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'invalid', STATE);

  await signIn.signIn({ email, password: newPassword });
  await expect(page).toHaveURL(YouthHomePage.urlPattern);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'youth-home');
});
