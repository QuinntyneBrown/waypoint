import { expect, test } from '@playwright/test';
import { OrganizationSettingsPage, SignInPage } from '../pages';
import { registerUser, seedYouthCurriculum } from '../support/api';

// Traces to: L2-ADMIN-006, L2-NFR-009, L2-NFR-010
test('organization settings controls and track rows reflow on a narrow screen', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'settings-mobile');
  await new SignInPage(page).signIn(admin);

  const settings = new OrganizationSettingsPage(page);
  await settings.goto();
  await expect(settings.track(seeded.primaryTrackId)).toBeVisible();
  expect((await settings.saveSettings.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await expect.poll(() => settings.horizontalOverflow()).toBeLessThanOrEqual(1);
});
