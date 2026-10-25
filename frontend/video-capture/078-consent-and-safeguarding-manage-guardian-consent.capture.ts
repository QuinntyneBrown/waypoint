import { expect, test } from '@playwright/test';
import {
  ConsentRequiredPage,
  GuardianConsentPage,
  RequestedGuardianConsentPage,
  SignInPage,
} from '../e2e/pages';
import { latestTestEmail } from '../e2e/support/api';
import { seedGuardianCast, shotElement } from './guardian-cast';
import { settle } from './guardian-cast';
import { shotOf } from './shotOf';

const FOLDER = '078-consent-and-safeguarding-manage-guardian-consent';

test('capture: manage guardian consent', async ({ page, request, browser }) => {
  test.setTimeout(180_000);
  const { guardian, family } = await seedGuardianCast(request, 'Imani Clarke', false);

  // The youth asks for a secure link; the guardian opens it from the email.
  await new SignInPage(page).signIn({ email: family.childEmail, password: family.childPassword });
  const required = new ConsentRequiredPage(page);
  await expect(required.heading).toBeVisible();
  await required.notifyGuardianButton.click();
  await expect(page.getByText(/secure consent link.*sent/i)).toBeVisible();
  const email = await latestTestEmail(request, guardian.email);
  const token = /token=([A-Za-z0-9_-]+)/.exec(email.body)?.[1];
  expect(token).toBeTruthy();

  const signer = await (await browser.newContext({ baseURL: 'http://localhost:4200' })).newPage();
  await signer.goto(`/consent/respond?token=${token}`);
  const form = new RequestedGuardianConsentPage(signer);
  await expect(form.heading).toBeVisible();
  await expect(signer.getByText('Imani Clarke').first()).toBeVisible();
  await form.guardianName.fill('Ruth Clarke');
  await form.relationship.fill('Mother');
  await form.participationOption.click();
  await form.mediaConsentOption.click();
  await form.groupOnlyOption.click();
  await form.signature.fill('Ruth Clarke');
  await shotElement(signer, FOLDER, 'form', signer.locator('section.ph-card').first());

  await form.submitButton.click();
  await expect(signer.getByRole('heading', { name: /consent recorded/i })).toBeVisible();
  await expect(form.reference).toContainText(/^GC-/);
  await shotOf(signer, FOLDER, 'recorded', signer.locator('section.ph-card').first(), 12);

  // Later, the guardian reviews the record in the portal and withdraws it.
  await page.context().clearCookies();
  await page.goto('/auth/sign-in');
  await page.evaluate(() => {
    localStorage.clear();
    sessionStorage.clear();
  });
  await new SignInPage(page).signIn(guardian);
  const consent = new GuardianConsentPage(page);
  await consent.goto();
  await expect(consent.participationStatus).toContainText(/active/i);
  await expect(consent.groupMedia).toBeChecked();
  await page.waitForLoadState('networkidle');
  const record = page.locator('section.ph-card', { hasText: 'Participation consent' });
  await shotOf(page, FOLDER, 'record', record, 12);
  await shotOf(
    page,
    FOLDER,
    'media',
    page.locator('section.ph-card', { hasText: 'Media consent' }),
    12,
  );

  page.once('dialog', (dialog) => dialog.accept());
  await consent.withdrawButton.click();
  await expect(consent.participationStatus).toContainText(/withdrawn/i);
  await expect(consent.participationStatus).not.toHaveCSS('background-color', 'rgba(0, 0, 0, 0)');
  await page.evaluate(() => window.scrollTo(0, 0));
  await settle(page);
  await shotOf(page, FOLDER, 'withdrawn', record, 12);
});
