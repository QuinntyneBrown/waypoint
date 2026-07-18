import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { SignInPage, SupportPage } from '../../pages';

// Traces to: L2-PLATFORM-005
test('a support request is submitted and acknowledged', async ({ page, request }) => {
  const creds = await registerYouth(request, 'support');
  await new SignInPage(page).signIn(creds);

  const support = new SupportPage(page);
  await support.goto();

  await support.message.fill('I can’t find my lessons anywhere.');
  // Category defaults to the first option; reply-to email is prefilled from the session.
  await support.sendButton.click();

  await expect(support.acknowledgementHeading).toBeVisible();
});
