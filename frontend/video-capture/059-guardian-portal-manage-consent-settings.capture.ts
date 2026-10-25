import { expect, test } from '@playwright/test';
import { GuardianConsentPage, SignInPage } from '../e2e/pages';
import { seedGuardianCast, shotWithMessage } from './guardian-cast';
import { shotOf } from './shotOf';

const FOLDER = '059-guardian-portal-manage-consent-settings';

test('capture: manage consent settings', async ({ page, request }) => {
  const { guardian } = await seedGuardianCast(request, 'Imani Clarke');

  await new SignInPage(page).signIn(guardian);
  const consent = new GuardianConsentPage(page);
  await consent.goto();
  await expect(consent.participationStatus).toContainText(/active/i);
  const record = page.locator('section.ph-card', { hasText: 'Participation consent' });
  const media = page.locator('section.ph-card', { hasText: 'Media consent' });
  await shotOf(page, FOLDER, 'record', record);

  await consent.mediaConsentOption.click();
  await consent.groupMediaOption.click();
  await consent.firstNameCaptionsOption.click();
  await shotOf(page, FOLDER, 'media-options', media);

  await consent.saveMediaButton.click();
  await expect(consent.status).toContainText(/media consent saved/i);
  await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
  await shotWithMessage(page, FOLDER, 'media-saved', page.locator('.ph-alert--success'));

  page.once('dialog', (dialog) => dialog.accept());
  await consent.withdrawButton.click();
  await expect(consent.participationStatus).toContainText(/withdrawn/i);
  await page.evaluate(() => window.scrollTo(0, 0));
  await shotOf(page, FOLDER, 'withdrawn', record);
});
