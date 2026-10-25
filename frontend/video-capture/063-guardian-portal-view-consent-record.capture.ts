import { expect, test } from '@playwright/test';
import { GuardianConsentPage, SignInPage } from '../e2e/pages';
import { latestTestEmail } from '../e2e/support/api';
import { seedGuardianCast, shotElement, shotWithMessage } from './guardian-cast';

const FOLDER = '063-guardian-portal-view-consent-record';

test('capture: view consent record', async ({ page, request, browser }) => {
  test.setTimeout(120_000);
  const { guardian } = await seedGuardianCast(request, 'Imani Clarke');

  await new SignInPage(page).signIn(guardian);
  const consent = new GuardianConsentPage(page);
  await consent.goto();
  await expect(consent.referenceId).toContainText(/CONSENT-/);
  const record = page.locator('section.ph-card', { hasText: 'Participation consent' });
  await shotElement(page, FOLDER, 'record', record);

  await consent.emailCopyButton.click();
  await expect(consent.status).toContainText(/consent record.*emailed/i);
  await shotWithMessage(page, FOLDER, 'emailed', page.locator('.ph-alert--success'));
  const email = await latestTestEmail(request, guardian.email);
  console.log('EMAIL SUBJECT:', email.subject, '|', email.body.slice(0, 400));

  const other = await (await browser.newContext({ baseURL: 'http://localhost:4200' })).newPage();
  const emptyEmail = `ruth-${crypto.randomUUID().slice(0, 8)}@waypoint.demo`;
  await request.post('/api/_test/users', {
    data: {
      email: emptyEmail,
      password: 'Password123',
      displayName: 'Ruth Clarke',
      role: 'Guardian',
    },
  });
  await new SignInPage(other).signIn({ email: emptyEmail, password: 'Password123' });
  await new GuardianConsentPage(other).goto();
  await expect(other.getByRole('heading', { name: /no consent record yet/i })).toBeVisible();
  const title = (await other.locator('.ph-empty__title').boundingBox())!;
  await other.setViewportSize({ width: 1280, height: 800 });
  await other.screenshot({
    path: `../docs/videos/${FOLDER}/images/empty-1280.png`,
    clip: { x: title.x + title.width / 2 - 200, y: title.y - 120, width: 400, height: 240 },
  });
});
