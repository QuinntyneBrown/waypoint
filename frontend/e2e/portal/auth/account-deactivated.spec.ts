import { expect, test } from '@playwright/test';
import { AccountDeactivatedPage, ContactPage } from '../../pages';

// Traces to: L2-PLATFORM-010
test('a deactivated user can contact the director without entering an authenticated route', async ({
  page,
}) => {
  const inactive = new AccountDeactivatedPage(page);
  await inactive.goto();
  await expect(inactive.heading).toBeVisible();

  await inactive.contactDirectorLink.click();

  await expect(page).toHaveURL('http://localhost:4201/contact');
  await expect(new ContactPage(page).heading).toBeVisible();
});
