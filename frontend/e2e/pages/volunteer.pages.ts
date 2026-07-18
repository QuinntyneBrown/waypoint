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
  get takeAttendanceLink() {
    return this.page.getByRole('link', { name: /take attendance/i });
  }

  async goto(): Promise<void> {
    await this.page.goto(VolunteerTodayPage.path);
  }
}

/** `/volunteer/attendance/:sessionId` — names-only Present/Absent capture. */
export class VolunteerAttendancePage extends BasePage {
  static readonly urlPattern = /\/volunteer\/attendance\/[0-9a-f-]+/i;

  get heading() {
    return this.page.getByRole('heading', { name: /take attendance/i });
  }
  get rosterRows() {
    return this.page.locator('[data-testid="attendance-row"]');
  }
  get allPresentButton() {
    return this.page.getByRole('button', { name: /all present/i });
  }
  get saveButton() {
    return this.page.getByRole('button', { name: /save attendance/i });
  }
  get markedTally() {
    return this.page.getByTestId('marked-tally');
  }
  get presentTally() {
    return this.page.getByTestId('present-tally');
  }
  get absentTally() {
    return this.page.getByTestId('absent-tally');
  }
  get syncStatus() {
    return this.page.getByRole('status');
  }

  rowFor(name: string) {
    return this.rosterRows.filter({ hasText: name });
  }

  async markAbsent(name: string): Promise<void> {
    await this.rowFor(name)
      .getByRole('button', { name: /absent/i })
      .click();
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
