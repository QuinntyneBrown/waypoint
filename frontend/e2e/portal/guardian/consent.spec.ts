import { expect, test } from '@playwright/test';
import { GuardianConsentPage, SignInPage } from '../../pages';
import { registerUser, seedGuardianFamily } from '../../support/api';

// Traces to: L2-GUARDIAN-003, L2-CONSENT-001
test('a guardian manages contact/media consent and receives their e-sign record', async ({
  page,
  request,
}) => {
  const guardian = await registerUser(request, 'Guardian', 'guardian-consent');
  await seedGuardianFamily(request, guardian.userId);
  await new SignInPage(page).signIn(guardian);

  const consent = new GuardianConsentPage(page);
  await consent.goto();
  await expect(consent.heading).toBeVisible();
  await expect(consent.participationStatus).toContainText(/active/i);
  await expect(consent.referenceId).toContainText(/CONSENT-/i);

  await consent.mediaConsentOption.click();
  await consent.groupMediaOption.click();
  await consent.firstNameCaptionsOption.click();
  await consent.saveMediaButton.click();
  await expect(consent.status).toContainText(/media consent saved/i);

  await consent.phone.fill('+14165550999');
  await consent.emergencyName.fill('Morgan Family');
  await consent.saveContactButton.click();
  await expect(consent.status).toContainText(/contact details saved/i);

  await consent.emailCopyButton.click();
  await expect(consent.status).toContainText(/consent record.*emailed/i);

  await page.reload();
  await expect(consent.mediaConsent).toBeChecked();
  await expect(consent.groupMedia).toBeChecked();
  await expect(consent.firstNameCaptions).toBeChecked();
  await expect(consent.phone).toHaveValue('+14165550999');
  await expect(consent.emergencyName).toHaveValue('Morgan Family');

  page.once('dialog', async (dialog) => {
    expect(dialog.message()).toContain('withdraw participation and media consent');
    await dialog.accept();
  });
  await consent.withdrawButton.click();
  await expect(consent.participationStatus).toContainText(/withdrawn/i);
});

test('guardian consent shows a safe empty state without a linked child', async ({
  page,
  request,
}) => {
  const guardian = await registerUser(request, 'Guardian', 'guardian-consent-empty');
  await new SignInPage(page).signIn(guardian);
  const consent = new GuardianConsentPage(page);
  await consent.goto();

  await expect(page.getByRole('heading', { name: /no consent record yet/i })).toBeVisible();
});
