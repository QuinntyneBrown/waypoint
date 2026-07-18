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

export class MentorCompetencyPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { level: 1 });
  }
  get summary() {
    return this.page.getByTestId('competency-summary');
  }
  card(name: string) {
    return this.page.getByTestId('competency-card').filter({ hasText: name });
  }
  level(name: string) {
    return this.page.getByLabel(`${name} level`);
  }
  notes(name: string) {
    return this.page.getByLabel(`${name} notes`);
  }
  save(name: string) {
    return this.card(name).getByRole('button', { name: /save assessment/i });
  }
  get status() {
    return this.page.getByRole('status');
  }
}

export class MentorCheckInsPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { name: 'Check-ins', exact: true });
  }
  get youth() {
    return this.page.getByLabel('Youth', { exact: true });
  }
  get type() {
    return this.page.getByLabel('Type', { exact: true });
  }
  get engagement() {
    return this.page.getByLabel(/engagement \/ mood/i);
  }
  get goals() {
    return this.page.getByLabel(/goals discussed/i);
  }
  get prayer() {
    return this.page.getByLabel(/prayer requests/i);
  }
  get actions() {
    return this.page.getByLabel(/action items/i);
  }
  get followUp() {
    return this.page.getByLabel(/next follow-up/i);
  }
  get share() {
    return this.page.getByRole('checkbox', { name: /share this check-in/i });
  }
  get shareControl() {
    return this.page.locator('label.wu-check').filter({ hasText: /share this check-in/i });
  }
  get guardianInformed() {
    return this.page.getByRole('checkbox', { name: /guardian was informed/i });
  }
  get guardianInformedControl() {
    return this.page.locator('label.wu-check').filter({ hasText: /guardian was informed/i });
  }
  get save() {
    return this.page.getByRole('button', { name: /save check-in/i });
  }
  get status() {
    return this.page.getByRole('status');
  }
  get historyItems() {
    return this.page.getByTestId('check-in-history-item');
  }
  get filterType() {
    return this.page.getByLabel(/filter by type/i);
  }
}

export class MentorFlagsPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { name: 'At-risk youth', exact: true });
  }
  get flags() {
    return this.page.getByTestId('at-risk-flag');
  }
  flag(name: string) {
    return this.flags.filter({ hasText: name });
  }
  get youth() {
    return this.page.getByLabel('Youth', { exact: true });
  }
  get manualReason() {
    return this.page.getByLabel('Reason', { exact: true });
  }
  get raise() {
    return this.page.getByRole('button', { name: /raise flag/i });
  }
  get status() {
    return this.page.getByRole('status');
  }
}
