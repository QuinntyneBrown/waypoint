import { expect, test } from '@playwright/test';
import {
  ConsentRequiredPage,
  RequestedGuardianConsentPage,
  SignInPage,
  YouthHomePage,
} from '../../pages';
import { latestTestEmail, registerUser, seedGuardianFamily } from '../../support/api';

// Traces to: L2-CONSENT-001, L2-CONSENT-002, L2-PLATFORM-011
test('blocked youth sends a secure link and guardian e-signature unblocks participation', async ({
  page,
  request,
}) => {
  const guardian = await registerUser(request, 'Guardian', 'consent-request');
  const family = await seedGuardianFamily(request, guardian.userId, { includeConsent: false });

  await new SignInPage(page).signIn({
    email: family.childEmail,
    password: family.childPassword,
  });
  const required = new ConsentRequiredPage(page);
  await expect(required.heading).toBeVisible();
  await required.notifyGuardianButton.click();
  await expect(page.getByText(/secure consent link.*sent/i)).toBeVisible();
  await expect(page.getByText(guardian.email, { exact: true })).toHaveCount(0);

  const email = await latestTestEmail(request, guardian.email);
  const token = /token=([A-Za-z0-9_-]+)/.exec(email.body)?.[1];
  expect(token).toBeTruthy();

  await page.goto(`/consent/respond?token=${token}`);
  const consent = new RequestedGuardianConsentPage(page);
  await expect(consent.heading).toBeVisible();
  await expect(page.getByText(family.childName)).toBeVisible();
  await consent.guardianName.fill('Amara Okafor');
  await consent.relationship.fill('Mother');
  await consent.participationOption.click();
  await consent.mediaConsentOption.click();
  await consent.groupOnlyOption.click();
  await consent.signature.fill('Amara Okafor');
  await consent.submitButton.click();
  await expect(page.getByRole('heading', { name: /consent recorded/i })).toBeVisible();
  await expect(consent.reference).toContainText(/^GC-/);

  await new YouthHomePage(page).goto();
  await expect(page).toHaveURL(/\/youth\/home/);
  await expect(new YouthHomePage(page).welcomeHeading).toBeVisible();
});

test('the immediate guardian-sign path switches from the youth session to guardian sign-in', async ({
  page,
  request,
}) => {
  const guardian = await registerUser(request, 'Guardian', 'consent-sign-now');
  const family = await seedGuardianFamily(request, guardian.userId, { includeConsent: false });
  await new SignInPage(page).signIn({
    email: family.childEmail,
    password: family.childPassword,
  });

  const required = new ConsentRequiredPage(page);
  await expect(required.heading).toBeVisible();
  await required.guardianSignNowButton.click();

  await expect(page).toHaveURL(/\/auth\/sign-in.*returnUrl=%2Fguardian%2Fconsent/);
  await expect(new SignInPage(page).email).toBeVisible();
});
