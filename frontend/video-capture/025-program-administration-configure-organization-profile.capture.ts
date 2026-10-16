import { APIRequestContext, Locator, Page, expect, test } from '@playwright/test';
import { OrganizationSettingsPage, SignInPage } from '../e2e/pages';
import { SeededUser, latestTestEmail, seedGuardianFamily, signInApi } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '025-program-administration-configure-organization-profile';

async function seedUser(
  request: APIRequestContext,
  role: string,
  displayName: string,
): Promise<SeededUser> {
  const email = `${displayName.split(' ')[0].toLowerCase()}-${crypto.randomUUID().slice(0, 8)}@waypoint.demo`;
  const password = 'Password123';
  const res = await request.post('/api/_test/users', {
    data: { email, password, displayName, role },
  });
  expect(res.ok()).toBeTruthy();
  const { userId } = (await res.json()) as { userId: string };
  return { email, password, displayName, role, userId };
}

/** Crop a screenshot to an element, with a little padding. */
async function shotOf(page: Page, name: string, target: Locator, pad = 12): Promise<void> {
  await target.scrollIntoViewIfNeeded();
  const box = (await target.boundingBox())!;
  await shot(page, FOLDER, name, {
    x: Math.max(0, box.x - pad),
    y: Math.max(0, box.y - pad),
    width: Math.min(1280, box.width + pad * 2),
    height: Math.min(800, box.height + pad * 2),
  });
}

test('capture: configure organization profile', async ({ page, request }) => {
  await page.setViewportSize({ width: 1280, height: 800 });
  const ruth = await seedUser(request, 'Guardian', 'Ruth Clarke');
  const family = await seedGuardianFamily(request, ruth.userId, { includeConsent: false });
  const admin = await seedUser(request, 'ProgramAdministrator', 'Pat Morgan');
  await new SignInPage(page).signIn(admin);

  const settings = new OrganizationSettingsPage(page);
  await settings.goto();
  await expect(settings.organizationName).toBeVisible();
  await settings.organizationName.fill('Waypoint Youth Leadership');
  await settings.displayName.fill('Waypoint');
  await settings.city.fill('Toronto');
  await settings.contactEmail.fill('hello@waypoint.example');
  await page.waitForLoadState('networkidle');
  await shotOf(page, 'identity', page.locator('section[aria-labelledby="identity-heading"]'));

  await settings.saveSettings.click();
  await expect(settings.outcome).toContainText('saved and recorded');
  await page.evaluate(() => window.scrollTo(0, 0));
  await shot(page, FOLDER, 'saved', { x: 244, y: 90, width: 1036, height: 430 });

  const childAuth = await signInApi(request, {
    email: family.childEmail,
    password: family.childPassword,
  });
  const consentRequest = await request.post('/api/consent/request', {
    headers: { Authorization: `Bearer ${childAuth.accessToken}` },
  });
  expect(consentRequest.ok()).toBeTruthy();
  const email = await latestTestEmail(request, ruth.email);
  const token = /token=([A-Za-z0-9_-]+)/.exec(email.body)?.[1];
  expect(token).toBeTruthy();

  const guardian = await page.context().newPage();
  await guardian.goto(`/consent/respond?token=${token}`);
  await expect(guardian.getByRole('heading', { name: /guardian consent/ })).toBeVisible();
  await guardian.waitForLoadState('networkidle');
  await shot(guardian, FOLDER, 'guardian-form', { x: 40, y: 20, width: 1200, height: 400 });
});
