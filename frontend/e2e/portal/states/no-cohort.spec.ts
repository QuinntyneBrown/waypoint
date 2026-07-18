import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { SignInPage, YouthHomePage } from '../../pages';

// Traces to: L2-PLATFORM-011
test('an unplaced youth sees the no-cohort state on home with a message-mentor path', async ({
  page,
  request,
}) => {
  // Register-created youth are never placed in a cohort.
  const creds = await registerYouth(request, 'nocohort');
  await new SignInPage(page).signIn(creds);

  const home = new YouthHomePage(page);
  await expect(page).toHaveURL(YouthHomePage.urlPattern);
  await expect(home.noCohortHeading).toBeVisible();
  await expect(home.messageMentorLink).toBeVisible();
});
