import { Page, expect, test } from '@playwright/test';
import { InviteAcceptPage, OnboardingPage, SignInPage, YouthHomePage } from '../../e2e/pages';
import {
  latestTestEmail,
  registerUser,
  registerYouth,
  seedYouthCurriculum,
  signInApi,
} from '../../e2e/support/api';
import { shot } from '../shots';

const FOLDER = 'journeys/27-first-run-onboarding';

async function cardClip(page: Page) {
  await page.setViewportSize({ width: 1280, height: 800 });
  const box = await page.locator('.w-authcard').boundingBox();
  expect(box).not.toBeNull();
  const pad = 24;
  const y = Math.max(0, box!.y - pad);
  return {
    x: box!.x - pad,
    y,
    width: box!.width + pad * 2,
    height: Math.min(800 - y, box!.height + pad * 2),
  };
}

test('capture: journey 27 first run onboarding', async ({ page, request, browser }) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'onboard-issuer');
  const auth = await signInApi(request, admin);
  const email = `amara-${crypto.randomUUID()}@waypoint.demo`;
  const issued = await request.post('/api/invitations', {
    headers: { Authorization: `Bearer ${auth.accessToken}` },
    data: { email, role: 'Youth', cohortId: seeded.cohortId, mentorId: seeded.mentorId },
  });
  expect(issued.ok(), await issued.text()).toBeTruthy();
  const token = /token=([^\s]+)/.exec((await latestTestEmail(request, email)).body)?.[1];
  expect(token).toBeTruthy();

  const invite = new InviteAcceptPage(page);
  await invite.goto(token!);
  await invite.displayName.fill('Amara Bell');
  await invite.password.fill('Password123');
  await invite.confirmPassword.fill('Password123');
  await invite.agreeToGuidelines();
  await invite.joinButton.click();
  await expect(page).toHaveURL(OnboardingPage.urlPattern);

  const onboarding = new OnboardingPage(page);
  await expect(onboarding.displayName).toHaveValue('Amara Bell');
  await expect(onboarding.skipButton).toBeVisible();
  await onboarding.pronouns.fill('she/her');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'profile', await cardClip(page));

  await onboarding.next();
  await expect(page.getByText('You’re all set with a cohort and mentor.')).toBeVisible();
  await shot(page, FOLDER, 'cohort', await cardClip(page));

  await onboarding.next();
  await expect(onboarding.checkbox('Lesson reminders')).toBeChecked();
  await expect(onboarding.checkbox('Announcements')).not.toBeChecked();
  await page.evaluate(() => (document.documentElement.style.zoom = '0.8'));
  await page.waitForTimeout(300);
  await shot(page, FOLDER, 'notifications', await cardClip(page));

  await page.evaluate(() => (document.documentElement.style.zoom = ''));
  await onboarding.finish();
  await expect(page).toHaveURL(YouthHomePage.urlPattern);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'home');

  // A youth who has not been placed yet sees an honest cohort step.
  const youth = await registerYouth(request, 'unplaced', { onboarded: false });
  const other = await browser.newPage({
    baseURL: 'http://localhost:4200',
    viewport: { width: 1280, height: 800 },
  });
  await new SignInPage(other).signIn(youth);
  const wizard = new OnboardingPage(other);
  await expect(other).toHaveURL(OnboardingPage.urlPattern);
  await wizard.next();
  await expect(wizard.unplacedNotice).toBeVisible();
  await other.waitForLoadState('networkidle');
  await shot(other, FOLDER, 'unplaced', await cardClip(other));
});
