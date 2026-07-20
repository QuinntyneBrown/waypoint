import { defineConfig, devices } from '@playwright/test';

/**
 * E2E acceptance harness for the Word Up frontend.
 *
 * Topology (see docs/frontend-implementation-plan.md § "E2E acceptance harness"):
 *   - API      http://127.0.0.1:5211  (real backend, InMemory DB, Testing env)
 *   - portal   http://localhost:4200  (proxies /api -> 5211)
 *   - public-site http://localhost:4201 (added in M6)
 *
 * Both dev servers proxy `/api` to the API, so the browser only ever makes
 * same-origin requests and the 4200-only CORS policy never bites.
 */

const CI = !!process.env.CI;

/**
 * Focused NFR projects. Keeping the responsive/accessibility specs in their
 * own directory avoids multiplying the full role-based acceptance suite by
 * every supported breakpoint.
 */
const QUALITY_VIEWPORTS = [
  { name: 'xs', width: 375, height: 812 },
  { name: 'sm', width: 768, height: 900 },
  { name: 'lg', width: 992, height: 900 },
  { name: 'xl', width: 1200, height: 900 },
] as const;

// e2e-only signing key (>= 32 chars) — never a production secret.
const E2E_SIGNING_KEY = 'e2e-only-signing-key-do-not-use-in-production-0123456789';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: CI,
  retries: CI ? 1 : 0,
  workers: CI ? 2 : undefined,
  reporter: CI ? [['html', { open: 'never' }], ['list']] : 'list',
  use: {
    trace: 'on-first-retry',
  },
  projects: [
    {
      name: 'portal',
      testDir: './e2e/portal',
      use: { ...devices['Desktop Chrome'], baseURL: 'http://localhost:4200' },
    },
    {
      name: 'public-site',
      testDir: './e2e/public-site',
      use: { ...devices['Desktop Chrome'], baseURL: 'http://localhost:4201' },
    },
    {
      // Mobile-viewport a11y/responsive passes against the portal.
      name: 'portal-xs',
      testDir: './e2e/portal-xs',
      use: { ...devices['Pixel 7'], baseURL: 'http://localhost:4200' },
    },
    {
      name: 'youth-performance',
      testDir: './e2e/quality',
      testMatch: 'youth-performance.spec.ts',
      use: {
        ...devices['Desktop Chrome'],
        baseURL: 'http://localhost:4200',
        viewport: { width: 1365, height: 768 },
      },
    },
    ...QUALITY_VIEWPORTS.map(({ name, width, height }) => ({
      name: `quality-${name}-${width}`,
      testDir: './e2e/quality',
      testIgnore: 'youth-performance.spec.ts',
      use: {
        ...devices['Desktop Chrome'],
        baseURL: 'http://localhost:4200',
        viewport: { width, height },
      },
      metadata: { breakpoint: name, viewportWidth: width },
    })),
  ],
  webServer: [
    {
      command:
        'dotnet run --project ../backend/src/WordUp.Api --no-build --urls http://127.0.0.1:5211',
      url: 'http://127.0.0.1:5211/health',
      timeout: 120_000,
      reuseExistingServer: !CI,
      env: {
        ASPNETCORE_ENVIRONMENT: 'Testing',
        ConnectionStrings__Default: 'InMemory',
        Jwt__SigningKey: E2E_SIGNING_KEY,
      },
    },
    {
      command: 'npx ng serve portal --configuration production --port 4200',
      url: 'http://localhost:4200',
      timeout: 180_000,
      reuseExistingServer: !CI,
    },
    {
      command: 'npx ng serve public-site --port 4201',
      url: 'http://localhost:4201',
      timeout: 180_000,
      reuseExistingServer: !CI,
    },
  ],
});
