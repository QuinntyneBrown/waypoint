import { expect, test } from '@playwright/test';
import { GuardianConsentPage, SignInPage } from '../e2e/pages';
import { seedGuardianCast, shotWithMessage } from './guardian-cast';
import { shotFull } from './shotOf';

const FOLDER = '060-guardian-portal-manage-contact-details';

test('capture: manage contact details', async ({ page, request }) => {
  const { guardian } = await seedGuardianCast(request, 'Imani Clarke');

  await new SignInPage(page).signIn(guardian);
  const consent = new GuardianConsentPage(page);
  await consent.goto();
  await expect(consent.participationStatus).toContainText(/active/i);
  const contact = page.locator('section.w-card', { hasText: 'Contact details' });
  const box = (await contact.boundingBox())!;
  await shotFull(page, FOLDER, 'contact', {
    x: box.x - 12,
    y: box.y - 12,
    width: box.width + 24,
    height: box.height + 24,
  });

  await consent.phone.fill('+14165550142');
  await consent.emergencyName.fill('Daniel Clarke');
  await page.getByLabel(/emergency contact phone/i).fill('+14165550143');
  await consent.saveContactButton.click();
  await expect(consent.status).toContainText(/contact details saved/i);
  await shotWithMessage(page, FOLDER, 'saved', page.locator('.w-alert--success'));

  await page.reload();
  await expect(consent.phone).toHaveValue('+14165550142');
  const reloaded = (await page
    .locator('section.w-card', { hasText: 'Contact details' })
    .boundingBox())!;
  await shotFull(page, FOLDER, 'after-reload', {
    x: reloaded.x - 12,
    y: reloaded.y - 12,
    width: reloaded.width + 24,
    height: reloaded.height + 24,
  });
});
