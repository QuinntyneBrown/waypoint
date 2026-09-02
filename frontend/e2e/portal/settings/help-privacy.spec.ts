import { expect, test } from '@playwright/test';
import { registerUser, registerYouth, signInApi } from '../../support/api';
import { HelpPage, PrivacySettingsPage, SignInPage } from '../../pages';

// Traces to: L2-PLATFORM-005
test('help content is searchable and tailored to the signed-in role', async ({ page, request }) => {
  const volunteer = await registerUser(request, 'Volunteer', 'help-volunteer');
  await new SignInPage(page).signIn(volunteer);
  const help = new HelpPage(page);
  await help.goto();

  await expect(help.heading).toBeVisible();
  await expect(page.getByText(/only see one session roster/i)).toBeVisible();
  await expect(page.getByText(/invite someone to waypoint/i)).toHaveCount(0);
  await help.search.fill('offline');
  await expect(page.getByText(/take attendance/i)).toBeVisible();
});

// Traces to: L2-NFR-003
test('a verified access request unlocks the complete export and history loads past 25', async ({
  page,
  request,
}) => {
  const youth = await registerYouth(request, 'privacy-rights');
  const admin = await registerUser(request, 'ProgramAdministrator', 'privacy-admin');
  await new SignInPage(page).signIn(youth);
  const privacy = new PrivacySettingsPage(page);
  await privacy.goto();

  await expect(page.getByText(youth.email, { exact: true })).toBeVisible();
  await expect(privacy.downloadButton).toBeDisabled();
  await privacy.action.selectOption('access');
  await privacy.details.fill('Please provide a copy for my records.');
  await privacy.submitButton.click();
  await expect(page.getByRole('status')).toContainText(/request received.*reference/i);

  for (let index = 0; index < 26; index += 1) {
    const response = await request.post('/api/me/data-requests', {
      headers: { Authorization: `Bearer ${youth.auth.accessToken}` },
      data: { actionType: 'access', details: `Paging request ${index + 1}.` },
    });
    expect(response.ok(), `privacy request ${index + 1} failed`).toBeTruthy();
  }

  const adminAuth = await signInApi(request, admin);
  const pending = await request.get('/api/admin/data-requests?status=pending&pageSize=100', {
    headers: { Authorization: `Bearer ${adminAuth.accessToken}` },
  });
  expect(pending.ok()).toBeTruthy();
  const pendingPage = (await pending.json()) as {
    items: { id: string; subjectEmail: string; actionType: string }[];
  };
  const accessRequest = pendingPage.items.find(
    (item) => item.subjectEmail === youth.email && item.actionType === 'access',
  );
  expect(accessRequest).toBeDefined();
  const fulfilled = await request.post(`/api/admin/data-requests/${accessRequest!.id}/fulfill`, {
    headers: { Authorization: `Bearer ${adminAuth.accessToken}` },
    data: { resolutionNote: 'Identity verified for the export.' },
  });
  expect(fulfilled.ok()).toBeTruthy();

  await page.reload();
  await expect(page.getByText(/showing 25 of 27 requests/i)).toBeVisible();
  await privacy.loadMoreButton.click();
  await expect(page.getByText(/showing 27 of 27 requests/i)).toBeVisible();
  await expect(privacy.downloadButton).toBeEnabled();
  const downloadPromise = page.waitForEvent('download');
  await privacy.downloadButton.click();
  const download = await downloadPromise;
  expect(download.suggestedFilename()).toMatch(/^waypoint-personal-data-.*\.json$/);
});
