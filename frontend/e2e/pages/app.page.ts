import { BasePage } from './base.page';

/**
 * The portal application root. Used by the harness boot smoke to prove the app
 * mounts; not a user-facing surface, so it only exposes the root element.
 */
export class PortalAppPage extends BasePage {
  static readonly path = '/';

  get root() {
    return this.page.locator('wu-root');
  }

  async goto(): Promise<void> {
    await this.page.goto(PortalAppPage.path);
  }
}
