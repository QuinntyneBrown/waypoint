import { Page } from '@playwright/test';

/**
 * Shared base for every page object. Holds the Playwright `Page` and exposes
 * cross-cutting helpers. Concrete pages expose their locators as getters (kept
 * lazy so field-initialiser ordering under native class fields is never a
 * concern) plus higher-level action methods; specs keep the `expect(...)`
 * assertions so each acceptance criterion stays visible in the test.
 */
export abstract class BasePage {
  constructor(protected readonly page: Page) {}

  /** The document's `<meta http-equiv="Content-Security-Policy">` content, or null when absent. */
  contentSecurityPolicy(): Promise<string | null> {
    return this.page.evaluate(
      () =>
        document
          .querySelector('meta[http-equiv="Content-Security-Policy"]')
          ?.getAttribute('content') ?? null,
    );
  }

  /**
   * Starts recording Content Security Policy violations in every document the page loads. Call it
   * before navigating, then read them with `cspViolations()`.
   */
  async recordCspViolations(): Promise<void> {
    await this.page.addInitScript(() => {
      const found: string[] = [];
      (window as unknown as { __cspViolations: string[] }).__cspViolations = found;
      document.addEventListener('securitypolicyviolation', (event) =>
        found.push(`${event.violatedDirective} blocked ${event.blockedURI}`),
      );
    });
  }

  cspViolations(): Promise<string[]> {
    return this.page.evaluate(
      () => (window as unknown as { __cspViolations?: string[] }).__cspViolations ?? [],
    );
  }

  /**
   * Pixels the document overflows horizontally. `<= 1` means no horizontal
   * scroll (a 1px rounding allowance). Used by the responsive passes.
   */
  horizontalOverflow(): Promise<number> {
    return this.page.evaluate(
      () => document.documentElement.scrollWidth - document.documentElement.clientWidth,
    );
  }

  /** The page canvas colour: the computed `<body>` background (theme check). */
  canvasBackground(): Promise<string> {
    return this.page.evaluate(() => getComputedStyle(document.body).backgroundColor);
  }

  /** The `color-scheme` in effect on the document root (native controls). */
  colorScheme(): Promise<string> {
    return this.page.evaluate(() => getComputedStyle(document.documentElement).colorScheme);
  }
}
