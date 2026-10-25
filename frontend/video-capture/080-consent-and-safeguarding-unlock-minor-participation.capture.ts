import { expect, test } from '@playwright/test';
import {
  ConsentRequiredPage,
  RequestedGuardianConsentPage,
  SignInPage,
  YouthHomePage,
} from '../e2e/pages';
import { latestTestEmail } from '../e2e/support/api';
import { seedGuardianCast, shotElement } from './guardian-cast';
import { shotOf } from './shotOf';
import { shot } from './shots';

const FOLDER = '080-consent-and-safeguarding-unlock-minor-participation';

test('capture: unlock minor participation', async ({ page, request }) => {
  test.setTimeout(180_000);
  const { guardian, family } = await seedGuardianCast(request, 'Imani Clarke', false);

  await new SignInPage(page).signIn({ email: family.childEmail, password: family.childPassword });
  const required = new ConsentRequiredPage(page);
  await expect(required.heading).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'gate', { x: 244, y: 56, width: 1036, height: 600 });

  await required.notifyGuardianButton.click();
  await expect(page.getByText(/secure consent link.*sent/i)).toBeVisible();
  await shot(page, FOLDER, 'notified', { x: 244, y: 56, width: 1036, height: 744 });

  const email = await latestTestEmail(request, guardian.email);
  const token = /token=([A-Za-z0-9_-]+)/.exec(email.body)?.[1];
  expect(token).toBeTruthy();

  await page.goto(`/consent/respond?token=${token}`);
  const form = new RequestedGuardianConsentPage(page);
  await expect(form.heading).toBeVisible();
  await expect(page.getByText('Imani Clarke').first()).toBeVisible();
  await form.guardianName.fill('Ruth Clarke');
  await form.relationship.fill('Mother');
  await form.participationOption.click();
  await form.signature.fill('Ruth Clarke');
  await shotElement(page, FOLDER, 'form', page.locator('section.ph-card').first());

  await form.submitButton.click();
  await expect(page.getByRole('heading', { name: /consent recorded/i })).toBeVisible();
  await shotOf(page, FOLDER, 'recorded', page.locator('.ph-container.ph-page'), 0, 1280, 460);

  await new YouthHomePage(page).goto();
  await expect(page).toHaveURL(/\/youth\/home/);
  await expect(new YouthHomePage(page).welcomeHeading).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'unlocked');
});
