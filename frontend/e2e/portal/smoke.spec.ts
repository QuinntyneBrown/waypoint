import { test, expect } from '@playwright/test';

// M0 smoke — proves the harness boots end-to-end: the portal app renders and the
// real backend's /health endpoint is reachable through the dev-server proxy.
// Untraced by design (no L2 behaviour asserted; the traceability gate allows this).

test('portal boots and renders the app root', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('wu-root')).toBeAttached();
});

test('backend /health is reachable through the portal proxy', async ({ request }) => {
  const res = await request.get('/health');
  expect(res.ok()).toBeTruthy();
});
