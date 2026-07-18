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

/** API 403 state. */
export class ForbiddenPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { name: /don.t have access/i });
  }
}

/** API 500 state with retry/draft recovery. */
export class ServerErrorPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { name: /something went wrong/i });
  }
  get retryButton() {
    return this.page.getByRole('button', { name: /try again/i });
  }
}
