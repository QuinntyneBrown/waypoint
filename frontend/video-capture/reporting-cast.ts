import { APIRequestContext, expect } from '@playwright/test';
import { SeededUser } from '../e2e/support/api';

/** Seed a Program Administrator named Pat Morgan. */
export async function seedPat(request: APIRequestContext): Promise<SeededUser> {
  const email = `pat-${crypto.randomUUID()}@waypoint.demo`;
  const password = 'Password123';
  const displayName = 'Pat Morgan';
  const role = 'ProgramAdministrator';
  const res = await request.post('/api/_test/users', {
    data: { email, password, displayName, role },
  });
  expect(res.ok(), `seed admin failed: ${res.status()} ${await res.text()}`).toBeTruthy();
  const { userId } = (await res.json()) as { userId: string };
  return { email, password, displayName, role, userId };
}

/** Sign in, retrying once if the shared API is slow to answer. */
export async function signInWithRetry(
  page: import('@playwright/test').Page,
  creds: { email: string; password: string },
): Promise<void> {
  const { SignInPage } = await import('../e2e/pages');
  try {
    await new SignInPage(page).signIn(creds);
  } catch {
    await page.waitForTimeout(3000);
    await new SignInPage(page).signIn(creds);
  }
}
