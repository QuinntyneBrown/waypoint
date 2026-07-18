import { APIRequestContext, expect } from '@playwright/test';

export interface AuthResult {
  accessToken: string;
  expiresAt: string;
  refreshToken: string;
  userId: string;
  email: string;
  displayName: string;
  role: string;
}

export interface YouthCredentials {
  email: string;
  password: string;
  displayName: string;
  auth: AuthResult;
}

/**
 * Provisioning is invite-only per product, so no register UI exists. The
 * anonymous `POST /api/auth/register` endpoint is used purely as a Playwright
 * data channel to mint a fresh, active Youth per test (unique-data-per-test —
 * tests never assert on shared global state).
 */
export async function registerYouth(
  request: APIRequestContext,
  tag: string,
): Promise<YouthCredentials> {
  const email = `e2e-${tag}-${crypto.randomUUID()}@wordup.test`;
  const password = 'Password123';
  const displayName = `E2E ${tag}`;
  const res = await request.post('/api/auth/register', {
    data: { email, password, displayName },
  });
  expect(res.ok(), `register failed: ${res.status()} ${await res.text()}`).toBeTruthy();
  const auth = (await res.json()) as AuthResult;
  return { email, password, displayName, auth };
}
