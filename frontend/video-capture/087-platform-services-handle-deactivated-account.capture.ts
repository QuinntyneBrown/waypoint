import { expect, test } from '@playwright/test';
import { AccountDeactivatedPage, SignInPage } from '../e2e/pages';
import { shot } from './shots';

const FOLDER = '087-platform-services-handle-deactivated-account';
const CARD = { x: 340, y: 200, width: 600, height: 400 };

test('capture: handle deactivated account', async ({ page, request }) => {
  test.setTimeout(240000);
  const password = 'Password123';
  const adminEmail = `pat-${crypto.randomUUID().slice(0, 6)}@waypoint.demo`;
  const youthEmail = `david-${crypto.randomUUID().slice(0, 6)}@waypoint.demo`;
  const admin = await request.post('/api/_test/users', {
    data: {
      email: adminEmail,
      password,
      displayName: 'Pat Morgan',
      role: 'ProgramAdministrator',
    },
  });
  expect(admin.ok()).toBeTruthy();
  const youth = await request.post('/api/_test/users', {
    data: { email: youthEmail, password, displayName: 'David Osei', role: 'Youth' },
  });
  expect(youth.ok()).toBeTruthy();
  const { userId } = (await youth.json()) as { userId: string };

  const adminAuth = await request.post('/api/auth/sign-in', {
    data: { email: adminEmail, password },
  });
  const { accessToken } = (await adminAuth.json()) as { accessToken: string };
  const deactivated = await request.post(`/api/people/${userId}/deactivate`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });
  expect(deactivated.ok()).toBeTruthy();

  const signIn = new SignInPage(page);
  await signIn.goto();
  await signIn.email.fill(youthEmail);
  await signIn.password.fill(password);
  await shot(page, FOLDER, 'sign-in', { x: 380, y: 90, width: 520, height: 620 });
  await signIn.submitButton.click();

  const inactive = new AccountDeactivatedPage(page);
  await expect(inactive.heading).toBeVisible({ timeout: 30000 });
  await expect(inactive.contactDirectorLink).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'inactive', CARD);
});
