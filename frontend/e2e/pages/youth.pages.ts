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

/** `/youth/curriculum` — enrolled track switcher and ordered gated module map. */
export class YouthCurriculumPage extends BasePage {
  static readonly path = '/youth/curriculum';

  get heading() {
    return this.page.getByRole('heading', { name: /curriculum/i });
  }
  get trackOptions() {
    return this.page.locator('[data-testid="curriculum-track"]');
  }
  get modules() {
    return this.page.locator('[data-testid="curriculum-module"]');
  }
  get emptyState() {
    return this.page.getByRole('heading', { name: /not set up yet/i });
  }

  module(name: string) {
    return this.modules.filter({
      has: this.page.getByRole('heading', { name, exact: true }),
    });
  }

  async goto(): Promise<void> {
    await this.page.goto(YouthCurriculumPage.path);
  }
}
