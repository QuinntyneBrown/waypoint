import { expect, test } from '@playwright/test';
import { registerUser, registerYouth } from '../../support/api';
import {
  ForbiddenPage,
  NotFoundPage,
  PeopleDirectoryPage,
  ServerErrorPage,
  SignInPage,
  SupportPage,
} from '../../pages';

// Traces to: L2-PLATFORM-006
test('an API 403 opens the forbidden state', async ({ page, request }) => {
  const youth = await registerYouth(request, 'api-403');
  await new SignInPage(page).signIn(youth);

  await new PeopleDirectoryPage(page).goto();

  await expect(new ForbiddenPage(page).heading).toBeVisible();
  await expect(page).toHaveURL(/\/forbidden$/);
});

// Traces to: L2-PLATFORM-007
test('an API resource 404 opens the not-found state', async ({ page, request }) => {
  const admin = await registerUser(request, 'ProgramAdministrator', 'api-404');
  await new SignInPage(page).signIn(admin);
  await page.route('**/api/people**', (route) =>
    route.fulfill({ status: 404, contentType: 'application/json', body: '{"title":"Missing"}' }),
  );

  await new PeopleDirectoryPage(page).goto();

  await expect(new NotFoundPage(page).heading).toBeVisible();
  await expect(page).toHaveURL(/\/not-found$/);
});

// Traces to: L2-PLATFORM-008, L2-NFR-007
test('an API 500 opens the server-error state and restores the in-progress form on retry', async ({
  page,
  request,
}) => {
  const youth = await registerYouth(request, 'api-500');
  await new SignInPage(page).signIn(youth);
  const support = new SupportPage(page);
  await support.goto();
  const draft = 'Please preserve this detailed support draft.';
  await support.message.fill(draft);
  await page.route('**/api/support', (route) =>
    route.fulfill({
      status: 500,
      contentType: 'application/json',
      body: '{"title":"An unexpected error occurred."}',
    }),
  );

  await support.sendButton.click();

  const serverError = new ServerErrorPage(page);
  await expect(serverError.heading).toBeVisible();
  await expect(page).toHaveURL(/\/server-error$/);

  await serverError.retryButton.click();
  await expect(page).toHaveURL(/\/support$/);
  await expect(support.message).toHaveValue(draft);
  await expect(support.replyToEmail).toHaveValue(youth.email);
});
