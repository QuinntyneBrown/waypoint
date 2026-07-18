import { test, expect } from '@playwright/test';
import { PortalAppPage } from '../pages';

// M0 smoke — proves the harness boots end-to-end: the portal app renders and the
// real backend's /health endpoint is reachable through the dev-server proxy.
// Untraced by design (no L2 behaviour asserted; the traceability gate allows this).

test('portal boots and renders the app root', async ({ page }) => {
  const app = new PortalAppPage(page);
  await app.goto();
  await expect(app.root).toBeAttached();
});

// Pure API reachability check — no page surface, so no page object applies.
test('backend /health is reachable through the portal proxy', async ({ request }) => {
  const res = await request.get('/health');
  expect(res.ok()).toBeTruthy();
});
