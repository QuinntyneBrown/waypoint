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

export interface SeededVolunteerSession {
  sessionId: string;
  youth: { youthUserId: string; name: string }[];
}

export interface SeededGuardianFamily {
  childId: string;
  cohortId: string;
  childName: string;
  childEmail: string;
  childPassword: string;
  moduleName: string;
  earnedBadgeName: string;
  lockedBadgeName: string;
  upcomingTitle: string;
  sharedActionItems: string;
  privateSecret: string;
}

export interface SeededYouthCurriculum {
  youthId: string;
  youthEmail: string;
  youthPassword: string;
  primaryTrackId: string;
  primaryTrackName: string;
  secondTrackId: string;
  secondTrackName: string;
  firstModuleName: string;
  lockedModuleName: string;
  firstLessonTitle: string;
  firstLessonId: string;
  secondLessonTitle: string;
  secondLessonId: string;
  leaderSecret: string;
  quizId: string;
  quizTitle: string;
  firstQuizQuestion: string;
  secondQuizQuestion: string;
  firstCorrectAnswer: string;
  secondCorrectAnswer: string;
  quizBadgeName: string;
  mentorEmail: string;
  mentorPassword: string;
  assignmentId: string;
  assignmentTitle: string;
  assignmentCriterionId: string;
  assignmentCriterion: string;
  assignmentBadgeName: string;
  codingLessonId: string;
  codingLessonTitle: string;
  codingFirstStepTitle: string;
  codingSecondStepTitle: string;
  codingBadgeName: string;
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

/** Seed a same-day session assigned to an existing Volunteer (Testing only). */
export async function seedVolunteerSession(
  request: APIRequestContext,
  volunteerUserId: string,
  daysFromToday = 0,
): Promise<SeededVolunteerSession> {
  const res = await request.post('/api/_test/volunteer-sessions', {
    data: { volunteerUserId, daysFromToday },
  });
  expect(
    res.ok(),
    `seed volunteer session failed: ${res.status()} ${await res.text()}`,
  ).toBeTruthy();
  return (await res.json()) as SeededVolunteerSession;
}

/** Seed a linked child with aggregate progress data for an existing Guardian (Testing only). */
export async function seedGuardianFamily(
  request: APIRequestContext,
  guardianUserId: string,
  options: { includeConsent?: boolean } = {},
): Promise<SeededGuardianFamily> {
  const res = await request.post('/api/_test/guardian-families', {
    data: { guardianUserId, includeConsent: options.includeConsent ?? true },
  });
  expect(res.ok(), `seed guardian family failed: ${res.status()} ${await res.text()}`).toBeTruthy();
  return (await res.json()) as SeededGuardianFamily;
}

/** Read the latest captured transactional email in the Testing host. */
export async function latestTestEmail(
  request: APIRequestContext,
  to: string,
): Promise<{ to: string; subject: string; body: string }> {
  const res = await request.get(`/api/_test/emails/latest?to=${encodeURIComponent(to)}`);
  expect(res.ok(), `latest test email failed: ${res.status()} ${await res.text()}`).toBeTruthy();
  return (await res.json()) as { to: string; subject: string; body: string };
}

/** Seed a consented youth with multiple enrolled curriculum tracks (Testing only). */
export async function seedYouthCurriculum(
  request: APIRequestContext,
): Promise<SeededYouthCurriculum> {
  const res = await request.post('/api/_test/youth-curriculum');
  expect(
    res.ok(),
    `seed youth curriculum failed: ${res.status()} ${await res.text()}`,
  ).toBeTruthy();
  return (await res.json()) as SeededYouthCurriculum;
}

export interface SeededApplication {
  applicationId: string;
  firstName: string;
  lastName: string;
}

/**
 * Submit a public application through the real anonymous intake endpoint, with a
 * unique last name so a specific application can be located in the shared test DB.
 */
export async function submitApplication(
  request: APIRequestContext,
  tag = 'app',
): Promise<SeededApplication> {
  const firstName = 'Ada';
  const lastName = `E2E-${tag}-${crypto.randomUUID().slice(0, 8)}`;
  const res = await request.post('/api/applications', {
    data: {
      firstName,
      lastName,
      age: 15,
      whoIsFilling: 'guardian',
      neighbourhood: 'Peckham',
      cohortInterest: 'Peckham Autumn cohort',
      whyInterested: 'I want to grow as a leader and serve.',
      trackInterest: 'Bible & Leadership',
      guardianName: 'Ngozi Okafor',
      guardianEmail: 'guardian@example.com',
      guardianPhone: '4165550100',
      participationConsentAck: true,
      privacyAck: true,
    },
  });
  expect(res.ok(), `submit application failed: ${res.status()} ${await res.text()}`).toBeTruthy();
  const { applicationId } = (await res.json()) as { applicationId: string };
  return { applicationId, firstName, lastName };
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
