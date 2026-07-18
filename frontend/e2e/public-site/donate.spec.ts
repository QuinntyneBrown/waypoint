import { expect, test } from '@playwright/test';
import { DonatePage } from '../pages';

// Traces to: L2-PUB-003
test('selecting a monthly tier hands off to the external provider with no card data collected', async ({
  page,
}) => {
  const donate = new DonatePage(page);
  await donate.goto();

  // No card data is ever collected on our page.
  await expect(donate.cardInputs).toHaveCount(0);

  await donate.tier('$50/mo').click();
  await donate.continueButton.click();

  await expect(donate.thankYou).toBeVisible();
  await expect(donate.monthlySummary('50')).toBeVisible();

  await expect(donate.providerLink).toHaveAttribute('href', /example-pci-provider/);
  await expect(donate.providerLink).toHaveAttribute('target', '_blank');
});
