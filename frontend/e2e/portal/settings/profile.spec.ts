import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { ProfileSettingsPage, SignInPage } from '../../pages';

const profilePng = {
  name: 'profile.png',
  mimeType: 'image/png',
  buffer: Buffer.from(
    'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=',
    'base64',
  ),
};

// Traces to: L2-PLATFORM-002
test('a user updates their profile and sees a confirmation', async ({ page, request }) => {
  const creds = await registerYouth(request, 'profile');
  await new SignInPage(page).signIn(creds);

  const profile = new ProfileSettingsPage(page);
  await profile.goto();

  await profile.displayName.fill('Sam the Leader');
  await profile.pronouns.fill('they/them');
  await profile.saveProfileButton.click();

  await expect(profile.profileUpdatedToast).toBeVisible();
});

// Traces to: L2-PLATFORM-002 AC3, L2-NFR-001 AC4
test('a profile photo is validated, privately persisted, and removable', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'profile-photo');
  await new SignInPage(page).signIn(creds);
  const profile = new ProfileSettingsPage(page);
  await profile.goto();

  await profile.photoInput.setInputFiles({
    name: 'not-an-image.png',
    mimeType: 'image/png',
    buffer: Buffer.from('<script>alert(1)</script>'),
  });
  await expect(page.getByRole('alert')).toContainText(/valid jpg or png/i);

  const uploadedResponse = page.waitForResponse(
    (response) =>
      response.url().includes('/api/me/profile/photo') && response.request().method() === 'POST',
  );
  await profile.photoInput.setInputFiles(profilePng);
  const uploaded = await uploadedResponse;
  expect(uploaded.ok(), await uploaded.text()).toBeTruthy();
  const stored = (await uploaded.json()) as { url: string };
  expect(stored.url).toMatch(/^\/api\/profile-photos\//);
  await expect(profile.photoPreview).toBeVisible();

  await page.reload();
  await expect(profile.photoPreview).toBeVisible();
  await profile.removePhotoButton.click();
  await expect(profile.photoPreview).toHaveCount(0);

  const removed = await request.get(stored.url, {
    headers: { Authorization: `Bearer ${creds.auth.accessToken}` },
  });
  expect(removed.status()).toBe(404);
});

// Traces to: L2-PLATFORM-002
test('change-password enforces policy and blocks mismatches client-side', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'changepw');
  await new SignInPage(page).signIn(creds);

  const profile = new ProfileSettingsPage(page);
  await profile.goto();

  await profile.currentPassword.fill(creds.password);
  await profile.newPassword.fill('short');
  await profile.confirmNewPassword.fill('short');
  await expect(profile.passwordPolicyHint).toBeVisible();
  await expect(profile.changePasswordButton).toBeDisabled();

  await profile.newPassword.fill('NewPassword9');
  await profile.confirmNewPassword.fill('Mismatch9');
  await expect(profile.passwordMismatchError).toBeVisible();
  await expect(profile.changePasswordButton).toBeDisabled();

  await profile.confirmNewPassword.fill('NewPassword9');
  await expect(profile.changePasswordButton).toBeEnabled();
});
