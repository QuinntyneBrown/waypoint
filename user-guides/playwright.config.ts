import { defineConfig } from "@playwright/test";

/**
 * The guide is a static site with no build step and no server. Every spec loads
 * pages directly over `file://`, which is exactly how a reader may open them, so
 * there is deliberately no `webServer` here.
 *
 * `channel: 'chrome'` uses the Chrome installed on the machine rather than a
 * bundled browser, so the suite exercises a browser a reader actually has.
 *
 * The timeout is generous because several specs walk the entire guide — the
 * chrome check, the link check, the contents check and the narrow-viewport check
 * each load every registered page in turn.
 */
export default defineConfig({
  testDir: "./tests",
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  // One retry on CI only, matching frontend/playwright.config.ts. Locally the
  // suite runs without retries so a real failure is never hidden.
  retries: process.env.CI ? 1 : 0,
  workers: process.env.CI ? 2 : 4,
  timeout: 180_000,
  reporter: "line",
  use: {
    channel: "chrome",
    trace: "off",
  },
});
