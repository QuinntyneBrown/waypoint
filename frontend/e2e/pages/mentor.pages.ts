import { BasePage } from './base.page';
import { AppShell } from './shell';

/** `/mentor/dashboard` — scoped mentor summary, roster preview, and pending reviews. */
export class MentorDashboardPage extends BasePage {
  static readonly path = '/mentor/dashboard';

  get shell() {
    return new AppShell(this.page);
  }
  get heading() {
    return this.page.getByRole('heading', { name: /welcome back/i });
  }
  get youthCount() {
    return this.page.getByTestId('mentor-youth-count');
  }
  get pendingReviews() {
    return this.page.getByTestId('mentor-pending-reviews');
  }
  get averageProgress() {
    return this.page.getByTestId('mentor-average-progress');
  }
  get atRiskCount() {
    return this.page.getByTestId('mentor-at-risk-count');
  }
  get rosterPreview() {
    return this.page.getByTestId('mentor-roster-preview');
  }
  get reviewItems() {
    return this.page.getByTestId('mentor-review-item');
  }
  get viewAllYouth() {
    return this.page.getByRole('link', { name: /view all youth/i });
  }
  get noYouth() {
    return this.page.getByRole('heading', { name: /no youth assigned yet/i });
  }
  get noReviews() {
    return this.page.getByRole('heading', { name: /no reviews waiting/i });
  }

  async goto(): Promise<void> {
    await this.page.goto(MentorDashboardPage.path);
  }
}

/** `/mentor/my-youth` — searchable/filterable mentor-owned roster. */
export class MentorRosterPage extends BasePage {
  static readonly path = '/mentor/my-youth';

  get heading() {
    return this.page.getByRole('heading', { name: 'My youth', exact: true });
  }
  get search() {
    return this.page.getByLabel(/search youth/i);
  }
  get rows() {
    return this.page.getByTestId('mentor-roster-row');
  }
  get noMatches() {
    return this.page.getByRole('heading', { name: /no youth match/i });
  }
  get clearFilters() {
    return this.page.getByRole('button', { name: /clear filters/i });
  }
  filter(name: 'All' | 'On track' | 'At risk' | 'New') {
    return this.page.getByRole('button', { name, exact: true });
  }

  async goto(): Promise<void> {
    await this.page.goto(MentorRosterPage.path);
  }
}
