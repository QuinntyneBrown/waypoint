import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { OnboardingPage, SignInPage, YouthHomePage } from '../../pages';

// Traces to: L2-AUTH-011
test('a new youth completes the onboarding wizard and lands home', async ({ page, request }) => {
  const creds = await registerYouth(request, 'onboard', { onboarded: false });
  await new SignInPage(page).signIn(creds);
  await expect(page).toHaveURL(OnboardingPage.urlPattern);

  const onboarding = new OnboardingPage(page);

  // Step 1 — profile, display name prefilled from the account.
  await expect(onboarding.displayName).toHaveValue(creds.displayName);
  await onboarding.pronouns.fill('they/them');
  await onboarding.next();

  // Step 2 — cohort is read-only; a register-created youth is unplaced (no-cohort).
  await expect(onboarding.unplacedNotice).toBeVisible();
  await onboarding.next();

  // Step 3 — notification defaults: lessons on, mentor on, announcements off.
  await expect(onboarding.checkbox('Lesson reminders')).toBeChecked();
  await expect(onboarding.checkbox('Mentor messages')).toBeChecked();
  await expect(onboarding.checkbox('Announcements')).not.toBeChecked();

  await onboarding.finish();
  await expect(page).toHaveURL(YouthHomePage.urlPattern);
});

// Traces to: L2-AUTH-011
test('a new youth can skip onboarding and still land home', async ({ page, request }) => {
  const creds = await registerYouth(request, 'skip', { onboarded: false });
  await new SignInPage(page).signIn(creds);
  await expect(page).toHaveURL(OnboardingPage.urlPattern);

  await new OnboardingPage(page).skip();
  await expect(page).toHaveURL(YouthHomePage.urlPattern);
});
