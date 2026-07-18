import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { SignInPage, YouthHomePage } from '../../pages';

// Traces to: L2-PLATFORM-012
test('the offline banner appears when connectivity drops and clears when it returns', async ({
  page,
  context,
  request,
}) => {
  const creds = await registerYouth(request, 'offline');
  await new SignInPage(page).signIn(creds);

  const home = new YouthHomePage(page);
  await expect(page).toHaveURL(YouthHomePage.urlPattern);

  const banner = home.shell.offlineBanner;
  await expect(banner).not.toHaveClass(/is-visible/);

  // Content already loaded stays usable while offline (AC1).
  await home.shell.goOffline(context);
  await expect(banner).toHaveClass(/is-visible/);
  await expect(home.welcomeHeading).toBeVisible();

  // Connectivity returns → the indicator clears (AC3).
  await home.shell.goOnline(context);
  await expect(banner).not.toHaveClass(/is-visible/);
});
