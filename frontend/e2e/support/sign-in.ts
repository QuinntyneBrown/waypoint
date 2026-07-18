import { Page, expect } from '@playwright/test';
import { YouthCredentials } from './api';

/** Drive the sign-in form through the real UI. */
export async function signIn(page: Page, creds: Pick<YouthCredentials, 'email' | 'password'>) {
  await page.goto('/auth/sign-in');
  await page.getByLabel('Email').fill(creds.email);
  await page.getByLabel('Password', { exact: true }).fill(creds.password);
  await page.getByRole('button', { name: /sign in/i }).click();
  await expect(page).not.toHaveURL(/\/auth\/sign-in/);
}
