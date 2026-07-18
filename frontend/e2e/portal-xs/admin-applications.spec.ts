import { expect, test } from '@playwright/test';
import { ApplicationsListPage, SignInPage } from '../pages';
import { registerUser, submitApplication } from '../support/api';

// Traces to: L2-INTAKE-002, L2-NFR-009, L2-NFR-010
test('application review is contained and keyboard/touch operable on mobile', async ({
  page,
  request,
}) => {
  const admin = await registerUser(request, 'ProgramAdministrator', 'mobile-app-review');
  const application = await submitApplication(request, 'mobile-review');
  await new SignInPage(page).signIn(admin);
  const applications = new ApplicationsListPage(page);
  await applications.goto();

  expect(await applications.horizontalOverflow()).toBeLessThanOrEqual(1);
  await applications.review(application.lastName).click();
  await expect(applications.reviewDialog).toBeVisible();
  expect((await applications.decline.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await applications.decisionNote.focus();
  await page.keyboard.type('Reviewed on a small screen.');
  await page.keyboard.press('Escape');
  await expect(applications.reviewDialog).toHaveCount(0);
  expect(await applications.horizontalOverflow()).toBeLessThanOrEqual(1);
});
