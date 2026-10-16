import { expect, test } from '@playwright/test';
import {
  ConsentRequiredPage,
  GuardianConsentPage,
  RequestedGuardianConsentPage,
  SignInPage,
} from '../../e2e/pages';
import { latestTestEmail } from '../../e2e/support/api';
import { seedGuardianCast, shotElement } from '../guardian-cast';
import { shotOf } from '../shotOf';
import { shot } from '../shots';

const FOLDER = 'journeys/05-give-guardian-consent';

test('capture: give guardian consent', async ({ page, request, browser }) => {
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

  const ruth = await (await browser.newContext({ baseURL: 'http://localhost:4200' })).newPage();
  await ruth.goto(`/consent/respond?token=${token}`);
  const form = new RequestedGuardianConsentPage(ruth);
  await expect(form.heading).toBeVisible();
  await expect(ruth.getByText('Imani Clarke').first()).toBeVisible();
  await form.guardianName.fill('Ruth Clarke');
  await form.relationship.fill('Mother');
  await ruth.locator('span', { hasText: /i give permission/i }).click();
  await form.mediaConsentOption.click();
  await form.groupOnlyOption.click();
  await form.signature.fill('Ruth Clarke');
  await shotElement(ruth, FOLDER, 'form', ruth.locator('section.w-card').first());

  await form.submitButton.click();
  await expect(ruth.getByRole('heading', { name: /consent recorded/i })).toBeVisible();
  await shotOf(ruth, FOLDER, 'recorded', ruth.locator('.w-container.w-page'), 0, 1280, 460);

  await new SignInPage(ruth).goto();
  await new SignInPage(ruth).signIn(guardian);
  const consent = new GuardianConsentPage(ruth);
  await consent.goto();
  await expect(consent.participationStatus).toContainText(/active/i);
  await ruth.waitForLoadState('networkidle');
  const record = ruth.locator('section.w-card', { hasText: 'Participation consent' });
  await shotElement(ruth, FOLDER, 'record', record);

  ruth.once('dialog', (dialog) => dialog.accept());
  await consent.withdrawButton.click();
  await expect(consent.participationStatus).toContainText(/withdrawn/i);
  await ruth.evaluate(() => window.scrollTo(0, 0));
  await shotOf(ruth, FOLDER, 'withdrawn', record);
});
