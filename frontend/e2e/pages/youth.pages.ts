import { BasePage } from './base.page';
import { AppShell } from './shell';

/** `/youth/home` — the youth landing surface inside the app shell. */
export class YouthHomePage extends BasePage {
  static readonly path = '/youth/home';
  static readonly urlPattern = /\/youth\/home/;

  /** The surrounding app shell (sign-out, offline banner, bottom nav). */
  get shell() {
    return new AppShell(this.page);
  }

  get welcomeHeading() {
    return this.page.getByRole('heading', { name: /welcome/i });
  }
  /** Empty-state heading shown when the youth is not placed in a cohort. */
  get noCohortHeading() {
    return this.page.getByRole('heading', { name: /not in a cohort yet/i });
  }
  get messageMentorLink() {
    return this.page.getByRole('link', { name: /message your mentor/i });
  }

  async goto(): Promise<void> {
    await this.page.goto(YouthHomePage.path);
  }
}

/** `/youth/announcements` — the youth announcements feed. */
export class YouthAnnouncementsPage extends BasePage {
  static readonly path = '/youth/announcements';

  get emptyStateHeading() {
    return this.page.getByRole('heading', { name: /no announcements yet/i });
  }

  async goto(): Promise<void> {
    await this.page.goto(YouthAnnouncementsPage.path);
  }
}

/** `/youth/notifications` — the youth notifications feed. */
export class YouthNotificationsPage extends BasePage {
  static readonly path = '/youth/notifications';

  get emptyStateHeading() {
    return this.page.getByRole('heading', { name: /all caught up/i });
  }

  async goto(): Promise<void> {
    await this.page.goto(YouthNotificationsPage.path);
  }
}
