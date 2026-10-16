import { expect, Page } from '@playwright/test';
import { SignInPage } from '../e2e/pages';

/** Sign in through the real form, allowing for a busy shared API. */
export async function signInSlow(
  page: Page,
  creds: { email: string; password: string },
): Promise<void> {
  const signIn = new SignInPage(page);
  await signIn.goto();
  await signIn.submitCredentials(creds.email, creds.password);
  await expect(page).not.toHaveURL(/\/auth\/sign-in/, { timeout: 60_000 });
}
