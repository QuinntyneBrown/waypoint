import { expect, test } from '@playwright/test';

// Traces to: L2-PUB-003
test('selecting a monthly tier hands off to the external provider with no card data collected', async ({
  page,
}) => {
  await page.goto('/donate');

  // No card data is ever collected on our page.
  await expect(
    page.locator('input[autocomplete="cc-number"], input[name*="card" i], input[name*="cvc" i]'),
  ).toHaveCount(0);

  await page.getByRole('button', { name: '$50/mo' }).click();
  await page.getByRole('button', { name: /continue to secure checkout/i }).click();

  await expect(page.getByText(/thank you/i)).toBeVisible();
  await expect(page.getByText(/\$50 monthly/i)).toBeVisible();

  const providerLink = page.getByRole('link', { name: /continue to secure provider/i });
  await expect(providerLink).toHaveAttribute('href', /example-pci-provider/);
  await expect(providerLink).toHaveAttribute('target', '_blank');
});
