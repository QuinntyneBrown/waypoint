import { expect, test } from '@playwright/test';
import { registerYouth } from '../support/api';
import { ApplyPage, SignInPage, YouthHomePage } from '../pages';

// Traces to: L2-NFR-009
test('youth home is single-column with no horizontal overflow and a bottom nav on mobile', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'xs-home');
  await new SignInPage(page).signIn(creds);

  const home = new YouthHomePage(page);
  await expect(page).toHaveURL(YouthHomePage.urlPattern);

  // No element causes horizontal page overflow (AC1).
  expect(await home.horizontalOverflow()).toBeLessThanOrEqual(1);

  // Mobile-first youth navigation surfaces the bottom nav.
  await expect(home.shell.bottomNav).toBeVisible();
});

// Traces to: L2-NFR-009
test('the public application form has no horizontal overflow on mobile', async ({ page }) => {
  // The apply page lives on the public site; check it reflows on XS too.
  const apply = new ApplyPage(page);
  await apply.goto('http://localhost:4201');
  expect(await apply.horizontalOverflow()).toBeLessThanOrEqual(1);
});
