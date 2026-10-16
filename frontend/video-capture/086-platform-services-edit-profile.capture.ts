import { expect, Page, test } from '@playwright/test';
import { ProfileSettingsPage, SignInPage } from '../e2e/pages';
import { shot } from './shots';

/** Sign in through the real form, allowing for slow password hashing on a busy shared API. */
async function signInSlow(page: Page, email: string, password: string) {
  const signIn = new SignInPage(page);
  await signIn.goto();
  await signIn.submitCredentials(email, password);
  await expect(page).not.toHaveURL(/\/auth\/sign-in/, { timeout: 30000 });
}

const FOLDER = '086-platform-services-edit-profile';
const CONTENT = { x: 244, y: 80, width: 1036, height: 720 };

test('capture: edit profile', async ({ page, request }) => {
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
  const profile = new ProfileSettingsPage(page);
  await profile.goto();
  await expect(profile.displayName).toHaveValue(displayName);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'form', CONTENT);

  await profile.pronouns.fill('she/her');
  await page.getByLabel('Email').fill('not-an-email');
  await profile.saveProfileButton.click();
  await expect(page.getByText('Enter a valid email address.')).toBeVisible();
  await shot(page, FOLDER, 'invalid', CONTENT);

  await profile.photoInput.setInputFiles({
    name: 'not-an-image.png',
    mimeType: 'image/png',
    buffer: Buffer.from('this is not an image'),
  });
  const photoAlert = page.getByRole('alert');
  await expect(photoAlert).toContainText(/valid jpg or png/i);
  await photoAlert.evaluate((el) => el.scrollIntoView({ block: 'center' }));
  await shot(page, FOLDER, 'photo-error', CONTENT);

  await profile.goto();
  await expect(profile.displayName).toHaveValue(displayName);
  await page.waitForLoadState('networkidle');
  await profile.pronouns.fill('she/her');
  await profile.saveProfileButton.click();
  await expect(profile.profileUpdatedToast).toBeVisible({ timeout: 30000 });
  await shot(page, FOLDER, 'saved');
});
