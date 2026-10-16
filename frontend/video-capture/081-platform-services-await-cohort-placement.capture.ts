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

const FOLDER = '081-platform-services-await-cohort-placement';

test('capture: await cohort placement', async ({ page, request }) => {
  test.setTimeout(240000);
  const email = `amara-${crypto.randomUUID()}@waypoint.demo`;
  const password = 'Password123';
  const displayName = 'Amara Bell';
  const registered = await request.post('/api/auth/register', {
    data: { email, password, displayName },
  });
  expect(registered.ok()).toBeTruthy();
  const auth = (await registered.json()) as { accessToken: string };
  const onboarded = await request.post('/api/onboarding', {
    headers: { Authorization: `Bearer ${auth.accessToken}` },
    data: { displayName },
  });
  expect(onboarded.ok()).toBeTruthy();

  await signInSlow(page, email, password);
  const home = new YouthHomePage(page);
  await expect(page).toHaveURL(YouthHomePage.urlPattern);
  await expect(home.noCohortHeading).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'no-cohort', { x: 244, y: 80, width: 1036, height: 460 });

  await home.messageMentorLink.click();
  await expect(page).toHaveURL(/\/youth\/messages/);
  await expect(page.getByText('Loading messages')).toHaveCount(0);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'messages', { x: 244, y: 80, width: 1036, height: 380 });
});
