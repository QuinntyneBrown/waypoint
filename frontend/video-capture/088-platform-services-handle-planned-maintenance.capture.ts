import { expect, Page, test } from '@playwright/test';
import { SignInPage, YouthHomePage } from '../e2e/pages';
import { shot } from './shots';

/** Sign in through the real form, allowing for slow password hashing on a busy shared API. */
async function signInSlow(page: Page, email: string, password: string) {
  const signIn = new SignInPage(page);
  await signIn.goto();
  await signIn.submitCredentials(email, password);
  await expect(page).not.toHaveURL(/\/auth\/sign-in/, { timeout: 30000 });
}

const FOLDER = '088-platform-services-handle-planned-maintenance';
const CARD = { x: 340, y: 200, width: 600, height: 400 };

test('capture: handle planned maintenance', async ({ page, request }) => {
  test.setTimeout(240000);
  const email = `amara-${crypto.randomUUID().slice(0, 6)}@waypoint.demo`;
  const password = 'Password123';
  const displayName = 'Amara Bell';
  const registered = await request.post('/api/auth/register', {
    data: { email, password, displayName },
  });
  expect(registered.ok()).toBeTruthy();
  const auth = (await registered.json()) as { accessToken: string };
  await request.post('/api/onboarding', {
    headers: { Authorization: `Bearer ${auth.accessToken}` },
    data: { displayName },
  });

  await signInSlow(page, email, password);
  await expect(page).toHaveURL(YouthHomePage.urlPattern);
  await page.waitForLoadState('networkidle');

  // Maintenance mode is simulated by answering every API call with 503, as the server does.
  await page.route('**/api/**', (route) =>
    route.fulfill({ status: 503, headers: { 'Retry-After': '120' }, body: '' }),
  );
  await page.reload();
  const heading = page.getByRole('heading', { name: /we.ll be right back/i });
  await expect(heading).toBeVisible({ timeout: 30000 });
  await expect(page).toHaveURL(/\/maintenance/);
  await shot(page, FOLDER, 'maintenance', CARD);
});
