import { expect, test } from '@playwright/test';
import { registerYouth } from '../support/api';
import { signIn } from '../support/sign-in';

// Traces to: L2-NFR-009
test('youth home is single-column with no horizontal overflow and a bottom nav on mobile', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'xs-home');
  await signIn(page, creds);
  await expect(page).toHaveURL(/\/youth\/home/);

  // No element causes horizontal page overflow (AC1).
  const overflow = await page.evaluate(
    () => document.documentElement.scrollWidth - document.documentElement.clientWidth,
  );
  expect(overflow).toBeLessThanOrEqual(1);

  // Mobile-first youth navigation surfaces the bottom nav.
  await expect(page.locator('.wu-botnav')).toBeVisible();
});

// Traces to: L2-NFR-009
test('the public application form has no horizontal overflow on mobile', async ({ page }) => {
  // The apply page lives on the public site; check it reflows on XS too.
  await page.goto('http://localhost:4201/apply');
  const overflow = await page.evaluate(
    () => document.documentElement.scrollWidth - document.documentElement.clientWidth,
  );
  expect(overflow).toBeLessThanOrEqual(1);
});
