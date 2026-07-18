import { expect, test } from '@playwright/test';
import { NotFoundPage } from '../../pages';

// Traces to: L2-PLATFORM-007
test('an unknown route shows the 404 state with a path back to safety', async ({ page }) => {
  const notFound = new NotFoundPage(page);
  await notFound.goto('/definitely/not/a/real/page');

  await expect(notFound.heading).toBeVisible();
  await expect(notFound.backToSafetyLink).toBeVisible();
});
