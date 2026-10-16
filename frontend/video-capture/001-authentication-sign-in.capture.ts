import { expect, test } from '@playwright/test';
import { AccountLockedPage, SignInPage, YouthHomePage } from '../e2e/pages';
import { shot } from './shots';

const CARD = { x: 380, y: 90, width: 520, height: 620 };
const LOCKED = { x: 340, y: 240, width: 600, height: 320 };

const FOLDER = '001-authentication-sign-in';

test('capture: sign in', async ({ page, request, browser }) => {
  const email = `amara-${crypto.randomUUID()}@waypoint.demo`;
  const password = 'Password123';
  const seeded = await request.post('/api/_test/users', {
    data: { email, password, displayName: 'Amara Bell', role: 'Youth' },
  });
  expect(seeded.ok()).toBeTruthy();

  const signIn = new SignInPage(page);
  await signIn.goto();
  await expect(signIn.submitButton).toBeVisible();
  await shot(page, FOLDER, 'form', CARD);

  await page.getByText('Keep me signed in').click();
  await expect(page.getByText(/not recommended on shared/i)).toBeVisible();
  await shot(page, FOLDER, 'keep', CARD);
  await page.getByText('Keep me signed in').click();

  await signIn.email.fill(email);
  await signIn.password.fill('WrongPassword9');
  await signIn.submitButton.click();
  await expect(signIn.errorAlert).toBeVisible();
  await shot(page, FOLDER, 'error', CARD);

  await signIn.password.fill(password);
  await signIn.submitButton.click();
  await expect(page).toHaveURL(YouthHomePage.urlPattern);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'youth-home');

  // Lockout is shown from a fresh user so the failed count starts at zero.
  const lockedEmail = `locked-${crypto.randomUUID()}@waypoint.demo`;
  await request.post('/api/_test/users', {
    data: { email: lockedEmail, password, displayName: 'Imani Clarke', role: 'Youth' },
  });
  const fresh = await browser.newPage({ baseURL: 'http://localhost:4200' });
  const lockedSignIn = new SignInPage(fresh);
  await lockedSignIn.goto();
  await lockedSignIn.email.fill(lockedEmail);
  for (let i = 0; i < 5; i++) {
    await lockedSignIn.password.fill('WrongPassword9');
    await lockedSignIn.submitButton.click();
    if (i < 4) await expect(lockedSignIn.errorAlert).toBeVisible();
  }
  await expect(fresh).toHaveURL(AccountLockedPage.urlPattern);
  await shot(fresh, FOLDER, 'locked', LOCKED);
});
