import { expect, test } from '@playwright/test';

// Traces to: L2-PUB-001
test('the landing page renders for anonymous visitors with journey, impact and CTAs', async ({
  page,
}) => {
  const res = await page.goto('/');
  expect(res?.status()).toBe(200);
  await expect(page).not.toHaveURL(/sign-in/);

  await expect(page.getByRole('heading', { name: /young leaders who love jesus/i })).toBeVisible();

  // The 4-step journey (Apply → Join a cohort → Learn the modules → Lead & graduate).
  for (const step of ['Apply', 'Join a cohort', 'Learn the modules', 'Lead & graduate']) {
    await expect(page.getByRole('heading', { name: step, exact: true })).toBeVisible();
  }

  // Primary CTAs are present (Apply, Sign in, Get involved, Contact).
  await expect(page.getByRole('link', { name: 'Sign in' })).toBeVisible();
  await expect(page.getByRole('link', { name: 'Get involved' }).first()).toBeVisible();
  await expect(page.getByRole('link', { name: 'Contact' }).first()).toBeVisible();

  // The Apply CTA navigates.
  await page.getByRole('link', { name: 'Apply', exact: true }).first().click();
  await expect(page).toHaveURL(/\/apply/);
});
