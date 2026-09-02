import { expect, test } from '@playwright/test';
import { registerYouth, signInApi } from '../support/api';

const INTERACTIVE_BUDGET_MS = 3_000;
const TOTAL_BLOCKING_BUDGET_MS = 400;
const CPU_SLOWDOWN = 1.25;
const NETWORK_LATENCY_MS = 50;
const DOWNLOAD_BYTES_PER_SECOND = 2_000_000;
const UPLOAD_BYTES_PER_SECOND = 1_000_000;

// Acceptance Test
// Traces to: L2-NFR-008 AC3
test('primary youth screen is interactive within the Chromebook-class budget', async ({
  page,
  request,
}) => {
  const credentials = await registerYouth(request, 'performance-budget');
  const auth = await signInApi(request, credentials);
  const storedSession = JSON.stringify({
    version: 1,
    refreshToken: auth.refreshToken,
    user: {
      userId: auth.userId,
      email: auth.email,
      displayName: auth.displayName,
      role: auth.role,
    },
    remember: false,
  });

  await page.addInitScript((session) => {
    if (location.origin === 'http://localhost:4200') {
      sessionStorage.setItem('w.session.v1', session);
    }
    const metrics = globalThis as typeof globalThis & {
      __wordUpInteractiveAt?: number;
      __wordUpLongTasks?: Array<{ startTime: number; duration: number }>;
    };
    metrics.__wordUpLongTasks = [];
    try {
      new PerformanceObserver((entries) => {
        metrics.__wordUpLongTasks?.push(
          ...entries.getEntries().map((entry) => ({
            startTime: entry.startTime,
            duration: entry.duration,
          })),
        );
      }).observe({ type: 'longtask', buffered: true });
    } catch {
      // Long-task entries are available in the CI Chromium build; an absent API
      // leaves the duration list empty while the interactive budget still runs.
    }

    // Locator polling happens outside the page and can observe an already
    // interactive control up to one polling interval late. Capture the exact
    // in-page moment that the primary action enters the rendered layout, then
    // retain the locator assertion below as the user-facing correctness check.
    const captureInteractive = (): void => {
      if (metrics.__wordUpInteractiveAt !== undefined) return;
      const action = Array.from(document.querySelectorAll('a')).find((link) =>
        /message your mentor/i.test(link.textContent ?? ''),
      );
      if (!action) return;
      const styles = getComputedStyle(action);
      const bounds = action.getBoundingClientRect();
      if (
        styles.display !== 'none' &&
        styles.visibility !== 'hidden' &&
        bounds.width > 0 &&
        bounds.height > 0
      ) {
        metrics.__wordUpInteractiveAt = performance.now();
        observer.disconnect();
      }
    };
    const observer = new MutationObserver(captureInteractive);
    observer.observe(document, { childList: true, subtree: true, characterData: true });
    document.addEventListener('DOMContentLoaded', captureInteractive, { once: true });
  }, storedSession);

  const browser = await page.context().newCDPSession(page);
  await browser.send('Network.enable');
  await browser.send('Network.clearBrowserCache');
  await browser.send('Network.setCacheDisabled', { cacheDisabled: true });
  await browser.send('Network.emulateNetworkConditions', {
    offline: false,
    latency: NETWORK_LATENCY_MS,
    downloadThroughput: DOWNLOAD_BYTES_PER_SECOND,
    uploadThroughput: UPLOAD_BYTES_PER_SECOND,
    connectionType: 'cellular3g',
  });
  await browser.send('Emulation.setCPUThrottlingRate', { rate: CPU_SLOWDOWN });

  try {
    await page.goto('/youth/home', { waitUntil: 'domcontentloaded', timeout: 15_000 });
    const primaryAction = page.getByRole('link', { name: /message your mentor/i });
    await expect(primaryAction).toBeVisible({ timeout: 15_000 });
    await expect(primaryAction).toBeEnabled();

    const metrics = await page.evaluate(() => {
      const recorded = globalThis as typeof globalThis & {
        __wordUpInteractiveAt?: number;
        __wordUpLongTasks?: Array<{ startTime: number; duration: number }>;
      };
      const interactiveMs = recorded.__wordUpInteractiveAt ?? performance.now();
      const tasks = (recorded.__wordUpLongTasks ?? [])
        .map((task) => ({
          ...task,
          durationBeforeInteractive: Math.max(
            0,
            Math.min(task.startTime + task.duration, interactiveMs) - task.startTime,
          ),
        }))
        .filter((task) => task.startTime < interactiveMs);
      return {
        interactiveMs,
        totalBlockingMs: tasks.reduce(
          (total, task) => total + Math.max(0, task.durationBeforeInteractive - 50),
          0,
        ),
        longestTaskMs: tasks.length ? Math.max(...tasks.map((task) => task.duration)) : 0,
      };
    });
    console.log(`Youth performance metrics: ${JSON.stringify(metrics)}`);

    expect(metrics.interactiveMs, 'navigation-to-interactive time').toBeLessThanOrEqual(
      INTERACTIVE_BUDGET_MS,
    );
    expect(metrics.totalBlockingMs, 'total blocking time').toBeLessThanOrEqual(
      TOTAL_BLOCKING_BUDGET_MS,
    );
    test.info().annotations.push({
      type: 'performance',
      description:
        `interactive=${metrics.interactiveMs.toFixed(0)}ms; ` +
        `TBT=${metrics.totalBlockingMs.toFixed(0)}ms; ` +
        `longest=${metrics.longestTaskMs.toFixed(0)}ms; CPU=${CPU_SLOWDOWN}x; ` +
        `latency=${NETWORK_LATENCY_MS}ms`,
    });
  } finally {
    await browser.send('Emulation.setCPUThrottlingRate', { rate: 1 });
    await browser.send('Network.emulateNetworkConditions', {
      offline: false,
      latency: 0,
      downloadThroughput: -1,
      uploadThroughput: -1,
    });
    await browser.detach();
  }
});
