import { BasePage } from './base.page';

/** `/volunteer/home` — the volunteer "Today" surface. */
export class VolunteerTodayPage extends BasePage {
  static readonly path = '/volunteer/home';
  static readonly urlPattern = /\/volunteer\/home/;

  get heading() {
    return this.page.getByRole('heading', { name: /today.s session/i });
  }
  /** Empty-state heading shown when the volunteer has no session today. */
  get noSessionHeading() {
    return this.page.getByRole('heading', { name: /no session today/i });
  }

  async goto(): Promise<void> {
    await this.page.goto(VolunteerTodayPage.path);
  }
}

/** `/volunteer/schedule` — the volunteer's upcoming assigned sessions. */
export class VolunteerSchedulePage extends BasePage {
  static readonly path = '/volunteer/schedule';
  static readonly urlPattern = /\/volunteer\/schedule/;

  get heading() {
    return this.page.getByRole('heading', { name: /schedule/i });
  }
  /** Empty-state heading shown when the volunteer has no upcoming sessions. */
  get noSessionsHeading() {
    return this.page.getByRole('heading', { name: /no upcoming sessions/i });
  }

  async goto(): Promise<void> {
    await this.page.goto(VolunteerSchedulePage.path);
  }
}
