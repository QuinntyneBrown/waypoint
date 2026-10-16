import { APIRequestContext, expect } from '@playwright/test';
import { seedYouthCurriculum, signInApi } from '../e2e/support/api';

async function rename(
  request: APIRequestContext,
  creds: { email: string; password: string },
  displayName: string,
) {
  const auth = await signInApi(request, creds);
  const res = await request.put('/api/me/profile', {
    headers: { Authorization: `Bearer ${auth.accessToken}` },
    data: { displayName, email: creds.email, pronouns: null },
  });
  expect(res.ok(), `rename failed: ${res.status()} ${await res.text()}`).toBeTruthy();
}

/** The curriculum seed, with its mentor and youth renamed to cast members. */
export async function seedEngagementCast(request: APIRequestContext) {
  const seeded = await seedYouthCurriculum(request);
  await rename(
    request,
    { email: seeded.mentorEmail, password: seeded.mentorPassword },
    'Grace Adeyemi',
  );
  await rename(request, { email: seeded.youthEmail, password: seeded.youthPassword }, 'Amara Bell');
  return { ...seeded, mentorName: 'Grace Adeyemi', youthName: 'Amara Bell' };
}

/** A Program Administrator named Pat Morgan. */
export async function seedPat(request: APIRequestContext) {
  const email = `pat-${crypto.randomUUID()}@waypoint.demo`;
  const password = 'Password123';
  const res = await request.post('/api/_test/users', {
    data: { email, password, displayName: 'Pat Morgan', role: 'ProgramAdministrator' },
  });
  expect(res.ok()).toBeTruthy();
  return { email, password };
}
