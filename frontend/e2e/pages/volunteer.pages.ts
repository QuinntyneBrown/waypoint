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
