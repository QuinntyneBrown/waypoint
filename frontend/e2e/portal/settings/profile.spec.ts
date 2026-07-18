import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { ProfileSettingsPage, SignInPage } from '../../pages';

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
