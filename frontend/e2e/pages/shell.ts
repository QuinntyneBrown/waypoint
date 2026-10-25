import { BrowserContext, Page } from '@playwright/test';

/**
 * The authenticated app shell (`ph-shell`) that wraps every signed-in surface:
 * the sign-out control, the connectivity banner, and the mobile bottom nav.
 * A component object rather than a full page — compose it from page objects
 * (e.g. `YouthHomePage.shell`) or construct it directly.
 */
export class AppShell {
  constructor(private readonly page: Page) {}

  /** The library shell element that hosts every signed-in page. */
  get host() {
    return this.page.locator('ph-shell');
  }

  get signOutButton() {
    return this.page.getByRole('button', { name: 'Sign out' });
  }

  /** The offline indicator; gains `is-visible` while connectivity is down. */
  get offlineBanner() {
    return this.page.locator('.ph-offline');
  }

  /** Mobile-first bottom navigation, present on XS youth surfaces. */
  get bottomNav() {
    return this.page.locator('.ph-botnav');
  }

  /** The "Skip to content" link: the first tab stop on every signed-in page. */
  get skipLink() {
    return this.page.getByRole('link', { name: 'Skip to content' });
  }

  /** The page's main landmark, the target of the skip link. */
  get main() {
    return this.page.getByRole('main');
  }

  async signOut(): Promise<void> {
    await this.signOutButton.click();
  }

  /** Drop connectivity and fire the browser `offline` event the app listens for. */
  async goOffline(context: BrowserContext): Promise<void> {
    await context.setOffline(true);
    await this.page.evaluate(() => window.dispatchEvent(new Event('offline')));
  }

  /** Restore connectivity and fire the matching `online` event. */
  async goOnline(context: BrowserContext): Promise<void> {
    await context.setOffline(false);
    await this.page.evaluate(() => window.dispatchEvent(new Event('online')));
  }
}
