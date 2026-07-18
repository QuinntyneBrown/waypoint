import { expect, test } from '@playwright/test';
import { registerUser, submitApplication } from '../../support/api';
import { ApplicationsListPage, SignInPage } from '../../pages';

// Traces to: L2-INTAKE-002
test('an admin sees a submitted application in the review queue', async ({ page, request }) => {
  const admin = await registerUser(request, 'ProgramAdministrator', 'admin-apps');
  const application = await submitApplication(request, 'review');
  await new SignInPage(page).signIn(admin);

  const applications = new ApplicationsListPage(page);
  await applications.goto();
  await expect(page).toHaveURL(ApplicationsListPage.urlPattern);

  // The anonymously-submitted application appears in the admin review queue (AC1).
  await expect(applications.rowFor(application.lastName)).toBeVisible();
});
