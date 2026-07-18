import { expect, test } from '@playwright/test';

// Traces to: L2-PLATFORM-007
test('an unknown route shows the 404 state with a path back to safety', async ({ page }) => {
  await page.goto('/definitely/not/a/real/page');

  await expect(page.getByRole('heading', { name: /page not found/i })).toBeVisible();
  await expect(page.getByRole('link', { name: /back to safety/i })).toBeVisible();
});
