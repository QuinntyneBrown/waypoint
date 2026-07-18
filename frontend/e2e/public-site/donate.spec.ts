import { expect, test } from '@playwright/test';
import { DonatePage } from '../pages';

// Traces to: L2-PUB-003
test('a monthly pledge is submitted to the API and hands off to the external provider with no card data', async ({
  page,
}) => {
  const donate = new DonatePage(page);
  await donate.goto();

  // No card data is ever collected on our page.
  await expect(donate.cardInputs).toHaveCount(0);

  await donate.donorName.fill('Dana Donor');
  await donate.email.fill('dana@example.com');
  await donate.tier('$50/mo').click();

  // Confirming records the donor request server-side (L2-PUB-003 AC3) ...
  const pledgeRequest = page.waitForRequest(
    (r) => r.url().includes('/api/public/donation') && r.method() === 'POST',
  );
  await donate.continueButton.click();
  const body = (await pledgeRequest).postDataJSON();
  expect(body.monthlyAmount).toBe(50);
  expect(body.frequency).toBe('monthly');
  // ... but never any card/PAN data (handled by the external provider).
  expect(JSON.stringify(body)).not.toMatch(/card|cvc|pan/i);

  await expect(donate.thankYou).toBeVisible();
  await expect(donate.monthlySummary('50')).toBeVisible();

  await expect(donate.providerLink).toHaveAttribute('href', /example-pci-provider/);
  await expect(donate.providerLink).toHaveAttribute('target', '_blank');
});

// Traces to: L2-PUB-003
test('a pledge is blocked without donor name and a valid email', async ({ page }) => {
  const donate = new DonatePage(page);
  await donate.goto();

  await donate.email.fill('not-an-email');
  await donate.tier('$50/mo').click();
  await donate.continueButton.click();

  await expect(donate.nameError).toBeVisible();
  await expect(donate.emailError).toBeVisible();
  await expect(donate.thankYou).toHaveCount(0);
});
