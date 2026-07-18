import { expect, test } from '@playwright/test';

// Traces to: L2-INTAKE-001
test('a minor applicant submits a valid application and sees confirmation (no account yet)', async ({
  page,
}) => {
  await page.goto('/apply');
  await page.getByLabel('First name').fill('Sam');
  await page.getByLabel('Last name').fill('Rivera');
  await page.getByLabel('Age').fill('15');

  // Under-18 → guardian fields appear and are required.
  await expect(page.getByLabel('Guardian name')).toBeVisible();

  await page.getByLabel('Neighbourhood').fill('Rexdale');
  await page.getByLabel(/why are you interested/i).fill('I want to grow as a leader and serve.');
  await page.getByLabel('Guardian name').fill('Pat Rivera');
  await page.getByLabel('Guardian email').fill('pat.rivera@example.com');
  await page.getByLabel('Guardian phone').fill('416-555-0134');

  // The native checkbox is visually hidden behind a custom box; toggle via the label.
  await page.locator('label.wu-check', { hasText: 'participation' }).click();
  await page
    .locator('label.wu-check', { hasText: 'privacy notice' })
    .locator('.wu-check__box')
    .click();
  await expect(page.getByRole('checkbox', { name: /participation/i })).toBeChecked();
  await expect(page.getByRole('checkbox', { name: /privacy notice/i })).toBeChecked();

  await page.getByRole('button', { name: /submit application/i }).click();

  await expect(page.getByRole('heading', { name: /application received/i })).toBeVisible();
  await expect(page.getByText(/account/i)).toBeVisible();
});

// Traces to: L2-INTAKE-001
test('an age outside 12–18 is blocked with the eligible-ages message', async ({ page }) => {
  await page.goto('/apply');
  await page.getByLabel('Age').fill('20');
  await expect(page.getByText(/12.18 inclusive/i)).toBeVisible();
});

// Traces to: L2-INTAKE-001
test('missing consent acknowledgements block submission with field errors', async ({ page }) => {
  await page.goto('/apply');
  await page.getByLabel('First name').fill('Sam');
  await page.getByLabel('Last name').fill('Rivera');
  await page.getByLabel('Age').fill('16');
  await page.getByLabel('Neighbourhood').fill('Rexdale');
  await page.getByLabel(/why are you interested/i).fill('Growth');
  await page.getByLabel('Guardian name').fill('Pat Rivera');
  await page.getByLabel('Guardian email').fill('pat.rivera@example.com');
  await page.getByLabel('Guardian phone').fill('416-555-0134');

  // Leave both acknowledgements unchecked.
  await page.getByRole('button', { name: /submit application/i }).click();

  await expect(page.getByText(/acknowledge participation consent/i)).toBeVisible();
  await expect(page.getByText(/acknowledge the privacy notice/i)).toBeVisible();
  await expect(page.getByRole('heading', { name: /application received/i })).toHaveCount(0);
});
