import { expect, test } from '@playwright/test';
import { GuardianConsentPage, GuardianProgressPage, SignInPage } from '../pages';
import { registerUser, seedGuardianFamily } from '../support/api';

// Traces to: L2-GUARDIAN-001, L2-GUARDIAN-003, L2-NFR-009, L2-NFR-010
test('guardian progress and consent stay contained and operable on a mobile viewport', async ({
  page,
  request,
}) => {
  const guardian = await registerUser(request, 'Guardian', 'guardian-mobile');
  await seedGuardianFamily(request, guardian.userId);
  await new SignInPage(page).signIn(guardian);

  await page.goto(GuardianProgressPage.path);
  await expect(new GuardianProgressPage(page).heading).toBeVisible();
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);

  const consent = new GuardianConsentPage(page);
  await consent.goto();
  await expect(consent.heading).toBeVisible();
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);

  for (const button of await page.getByRole('button').all()) {
    if (!(await button.isVisible())) continue;
    const box = await button.boundingBox();
    expect(box?.height ?? 0).toBeGreaterThanOrEqual(44);
    expect(box?.width ?? 0).toBeGreaterThanOrEqual(44);
  }

  await consent.phone.fill('+14165550123');
  await consent.saveContactButton.focus();
  await page.keyboard.press('Enter');
  await expect(consent.status).toContainText(/contact details saved/i);
});
