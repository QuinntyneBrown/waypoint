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

/** `/mentor/youth/:id` — assigned-youth record with privacy-scoped detail tabs. */
export class MentorYouthDetailPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { level: 1 });
  }
  get atRiskBanner() {
    return this.page.getByTestId('youth-at-risk-banner');
  }
  tab(name: 'Profile' | 'Progress' | 'Attendance' | 'Check-ins' | 'Submissions' | 'Competencies') {
    return this.page.getByRole('tab', { name, exact: true });
  }
  panel(name: string) {
    return this.page.getByRole('tabpanel', { name });
  }
}

export class MentorReviewsPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { name: 'Reviews', exact: true });
  }
  get items() {
    return this.page.getByTestId('mentor-review-queue-item');
  }
  get emptyState() {
    return this.page.getByRole('heading', { name: /no reviews waiting/i });
  }
  get approveAllQuizzes() {
    return this.page.getByRole('button', { name: /approve all quiz scores/i });
  }
  get status() {
    return this.page.getByRole('status');
  }
}

export class MentorSubmissionReviewPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { level: 1 });
  }
  get submittedCopy() {
    return this.page.getByTestId('submitted-copy');
  }
  get total() {
    return this.page.getByTestId('rubric-total');
  }
  score(criterion: string) {
    return this.page.getByLabel(new RegExp(criterion, 'i'));
  }
  get comment() {
    return this.page.getByLabel(/feedback for the youth/i);
  }
  get guardianNotify() {
    return this.page.getByRole('checkbox', { name: /notify guardian/i });
  }
  get saveDraft() {
    return this.page.getByRole('button', { name: /save draft/i });
  }
  get approve() {
    return this.page.getByRole('button', { name: /^approve$/i });
  }
  get requestRevision() {
    return this.page.getByRole('button', { name: /request revision/i });
  }
  get status() {
    return this.page.getByRole('status');
  }
}

export class MentorQuizReviewPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { level: 1 });
  }
  get autoScore() {
    return this.page.getByTestId('quiz-auto-score');
  }
  get overrideScore() {
    return this.page.getByLabel(/override score/i);
  }
  get approve() {
    return this.page.getByRole('button', { name: /approve score/i });
  }
  get auditRecord() {
    return this.page.getByTestId('quiz-approval-record');
  }
  get backToReviews() {
    return this.page.getByRole('link', { name: /back to reviews/i });
  }
}
