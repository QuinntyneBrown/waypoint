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
