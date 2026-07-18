import { BasePage } from './base.page';

/**
 * The 404 state rendered by the catch-all route. Not a fixed path — navigate to
 * any unknown URL to reach it.
 */
export class NotFoundPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { name: /page not found/i });
  }
  get backToSafetyLink() {
    return this.page.getByRole('link', { name: /back to safety/i });
  }

  async goto(path: string): Promise<void> {
    await this.page.goto(path);
  }
}
