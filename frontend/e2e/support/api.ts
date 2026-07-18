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

export interface SeededUser {
  email: string;
  password: string;
  displayName: string;
  role: string;
  userId: string;
}

/**
 * Provision an active user with an arbitrary role via the Testing-only seed
 * endpoint (`POST /api/_test/users`). Non-Youth roles have no self-service
 * registration, so this is how E2E reaches the Program Administrator, Mentor,
 * Guardian, and Volunteer portals. Tests then sign in through the real UI
 * (`SignInPage`) or drive the product APIs with a token from {@link signInApi}.
 */
export async function registerUser(
  request: APIRequestContext,
  role: string,
  tag = 'user',
): Promise<SeededUser> {
  const email = `e2e-${tag}-${crypto.randomUUID()}@wordup.test`;
  const password = 'Password123';
  const displayName = `E2E ${tag}`;
  const res = await request.post('/api/_test/users', {
    data: { email, password, displayName, role },
  });
  expect(res.ok(), `seed user failed: ${res.status()} ${await res.text()}`).toBeTruthy();
  const { userId } = (await res.json()) as { userId: string };
  return { email, password, displayName, role, userId };
}

/** Sign in via the real API and return the auth result (for API-level setup as a user). */
export async function signInApi(
  request: APIRequestContext,
  creds: { email: string; password: string },
): Promise<AuthResult> {
  const res = await request.post('/api/auth/sign-in', {
    data: { email: creds.email, password: creds.password },
  });
  expect(res.ok(), `sign-in failed: ${res.status()} ${await res.text()}`).toBeTruthy();
  return (await res.json()) as AuthResult;
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
  opts: { onboarded?: boolean } = {},
): Promise<YouthCredentials> {
  const email = `e2e-${tag}-${crypto.randomUUID()}@wordup.test`;
  const password = 'Password123';
  const displayName = `E2E ${tag}`;
  const res = await request.post('/api/auth/register', {
    data: { email, password, displayName },
  });
  expect(res.ok(), `register failed: ${res.status()} ${await res.text()}`).toBeTruthy();
  const auth = (await res.json()) as AuthResult;

  // Default: mark onboarding complete via the API so the youth lands on their
  // home surface. Pass { onboarded: false } to drive the onboarding wizard.
  if (opts.onboarded !== false) {
    const done = await request.post('/api/onboarding', {
      headers: { Authorization: `Bearer ${auth.accessToken}` },
      data: { displayName },
    });
    expect(done.ok(), `onboarding failed: ${done.status()} ${await done.text()}`).toBeTruthy();
  }

  return { email, password, displayName, auth };
}
