import { APIRequestContext, expect } from '@playwright/test';
import { SeededYouthCurriculum, seedYouthCurriculum, signInApi } from '../e2e/support/api';

export async function rename(
  request: APIRequestContext,
  creds: { email: string; password: string },
  displayName: string,
): Promise<void> {
  const auth = await signInApi(request, creds);
  const res = await request.put('/api/me/profile', {
    headers: { Authorization: `Bearer ${auth.accessToken}` },
    data: { displayName, email: creds.email, pronouns: null },
  });
  expect(res.ok(), `rename failed: ${res.status()} ${await res.text()}`).toBeTruthy();
}

/** Seed the curriculum fixture, then give its youth and mentor cast display names. */
export async function seedCast(
  request: APIRequestContext,
  youthName: string,
): Promise<SeededYouthCurriculum> {
  const seeded = await seedYouthCurriculum(request);
  await rename(request, { email: seeded.youthEmail, password: seeded.youthPassword }, youthName);
  await rename(
    request,
    { email: seeded.mentorEmail, password: seeded.mentorPassword },
    'Grace Adeyemi',
  );
  return { ...seeded, youthName, mentorName: 'Grace Adeyemi' };
}
