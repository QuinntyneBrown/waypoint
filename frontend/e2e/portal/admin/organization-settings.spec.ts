import { expect, test } from '@playwright/test';
import { OrganizationSettingsPage, SignInPage } from '../../pages';
import {
  latestTestEmail,
  registerUser,
  seedGuardianFamily,
  seedYouthCurriculum,
  signInApi,
} from '../../support/api';

// Traces to: L2-ADMIN-006 AC1, AC4, AC5; L2-CONSENT-001 AC5
test('saved organization identity and consent policy appear on the guardian consent flow', async ({
  page,
  request,
}) => {
  const guardian = await registerUser(request, 'Guardian', 'settings-guardian');
  const family = await seedGuardianFamily(request, guardian.userId, { includeConsent: false });
  const admin = await registerUser(request, 'ProgramAdministrator', 'org-settings');
  await new SignInPage(page).signIn(admin);

  const settings = new OrganizationSettingsPage(page);
  await settings.goto();
  await settings.organizationName.fill('Waypoint Toronto');
  await settings.displayName.fill('Waypoint T.O.');
  await settings.logoUrl.fill('https://example.com/waypoint-logo.png');
  await settings.city.fill('Toronto');
  await settings.contactEmail.fill('consent@waypoint.example');
  await settings.consentStatement.fill('I approve participation in the current Waypoint program.');
  await settings.reconsentEachTerm.check();
  await settings.requireMediaConsent.check();
  await settings.saveSettings.click();
  await expect(settings.outcome).toContainText('saved and recorded');

  await page.reload();
  await expect(settings.organizationName).toHaveValue('Waypoint Toronto');
  await expect(settings.displayName).toHaveValue('Waypoint T.O.');
  await expect(settings.reconsentEachTerm).toBeChecked();

  const childAuth = await signInApi(request, {
    email: family.childEmail,
    password: family.childPassword,
  });
  const consentRequest = await request.post('/api/consent/request', {
    headers: { Authorization: `Bearer ${childAuth.accessToken}` },
  });
  expect(consentRequest.ok(), await consentRequest.text()).toBeTruthy();
  const email = await latestTestEmail(request, guardian.email);
  const token = /token=([A-Za-z0-9_-]+)/.exec(email.body)?.[1];
  expect(token).toBeTruthy();

  await page.goto(`/consent/respond?token=${token}`);
  await expect(page.getByRole('heading', { name: 'Waypoint T.O. guardian consent' })).toBeVisible();
  await expect(
    page.getByText('I approve participation in the current Waypoint program.'),
  ).toBeVisible();
  await expect(page.getByText('consent@waypoint.example')).toBeVisible();
  await expect(page.getByText(/media requires recorded guardian permission/i)).toBeVisible();
});

// Traces to: L2-ADMIN-006 AC2, AC3, AC5
test('an admin creates a term and pauses or resumes a published track', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'term-track-settings');
  await new SignInPage(page).signIn(admin);

  const settings = new OrganizationSettingsPage(page);
  await settings.goto();
  const termName = `E2E Autumn ${crypto.randomUUID().slice(0, 8)}`;
  await settings.termName.fill(termName);
  await settings.termStart.fill('2026-09-01');
  await settings.termEnd.fill('2026-12-15');
  await settings.termStatus.selectOption('Upcoming');
  await settings.saveTerm.click();
  await expect(settings.term(termName)).toContainText('Upcoming');

  const track = settings.track(seeded.primaryTrackId);
  await expect(track).toContainText('Enabled');
  await track.getByRole('button', { name: 'Pause track' }).click();
  await expect(track).toContainText('Paused');
  await track.getByRole('button', { name: 'Enable track' }).click();
  await expect(track).toContainText('Enabled');
});
