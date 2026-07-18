import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { signIn } from '../../support/sign-in';

// Traces to: L2-PLATFORM-012
test('the offline banner appears when connectivity drops and clears when it returns', async ({
  page,
  context,
  request,
}) => {
  const creds = await registerYouth(request, 'offline');
  await signIn(page, creds);
  await expect(page).toHaveURL(/\/youth\/home/);

  const banner = page.locator('.wu-offline');
  await expect(banner).not.toHaveClass(/is-visible/);

  // Content already loaded stays usable while offline (AC1).
  await context.setOffline(true);
  await page.evaluate(() => window.dispatchEvent(new Event('offline')));
  await expect(banner).toHaveClass(/is-visible/);
  await expect(page.getByRole('heading', { name: /welcome/i })).toBeVisible();

  // Connectivity returns → the indicator clears (AC3).
  await context.setOffline(false);
  await page.evaluate(() => window.dispatchEvent(new Event('online')));
  await expect(banner).not.toHaveClass(/is-visible/);
});
