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
  reviewItem(title: string) {
    return this.reviewItems.filter({ hasText: title });
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

/** `/mentor/messages` — scoped youth inbox, private threads, and cohort broadcast. */
export class MentorMessagesPage extends BasePage {
  static readonly path = '/mentor/messages';

  get heading() {
    return this.page.getByRole('heading', { name: 'Messages', exact: true });
  }
  get realtimeStatus() {
    return this.page.getByTestId('mentor-realtime-status');
  }
  get thread() {
    return this.page.getByTestId('mentor-message-thread');
  }
  get composer() {
    return this.page.locator('#mentor-message-body');
  }
  get sendButton() {
    return this.page.getByRole('button', { name: 'Send message', exact: true });
  }
  get broadcastButton() {
    return this.page.getByTestId('open-broadcast');
  }
  get broadcastBody() {
    return this.page.getByLabel('Message', { exact: true });
  }
  get reviewBroadcast() {
    return this.page.getByRole('button', { name: /review broadcast/i });
  }
  get confirmBroadcast() {
    return this.page.getByTestId('confirm-broadcast');
  }
  conversation(youthName: string) {
    return this.page.getByRole('button').filter({ hasText: youthName });
  }

  async goto(): Promise<void> {
    await this.page.goto(MentorMessagesPage.path);
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
  get conflict() {
    return this.page.getByTestId('submission-conflict');
  }
  get loadLatest() {
    return this.page.getByRole('button', { name: /load latest review/i });
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
  cardById(id: string) {
    return this.page.locator('[data-testid="competency-card"][data-competency-id="' + id + '"]');
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
  levelById(id: string) {
    return this.page.locator('#level-' + id);
  }
  notesById(id: string) {
    return this.page.locator('#notes-' + id);
  }
  saveById(id: string) {
    return this.cardById(id).getByRole('button', { name: /save assessment/i });
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
    return this.page.locator('label.w-check').filter({ hasText: /share this check-in/i });
  }
  get guardianInformed() {
    return this.page.getByRole('checkbox', { name: /guardian was informed/i });
  }
  get guardianInformedControl() {
    return this.page.locator('label.w-check').filter({ hasText: /guardian was informed/i });
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

export class MentorAssignWorkPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { name: 'Assign work', exact: true });
  }
  get itemType() {
    return this.page.getByLabel(/what are you assigning/i);
  }
  get item() {
    return this.page.getByLabel('Item', { exact: true });
  }
  youthCheckbox(name: string) {
    return this.page.getByRole('checkbox', { name: new RegExp(name, 'i') });
  }
  youthControl(name: string) {
    return this.page.locator('label.w-check').filter({ hasText: name });
  }
  get dueDate() {
    return this.page.getByLabel('Due date', { exact: true });
  }
  get reminder() {
    return this.page.getByLabel('Reminder', { exact: true });
  }
  get message() {
    return this.page.getByLabel(/message to youth/i);
  }
  get review() {
    return this.page.getByRole('button', { name: /review & assign/i });
  }
  get dialog() {
    return this.page.getByRole('dialog');
  }
  get confirm() {
    return this.dialog.getByRole('button', { name: /assign to \d+ youth/i });
  }
  get status() {
    return this.page.getByRole('status');
  }
}

export class MentorCalendarPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { name: 'Calendar', exact: true });
  }
  get monthTab() {
    return this.page.getByRole('tab', { name: 'Month', exact: true });
  }
  get weekTab() {
    return this.page.getByRole('tab', { name: 'Week', exact: true });
  }
  get agendaTab() {
    return this.page.getByRole('tab', { name: 'Agenda', exact: true });
  }
  get month() {
    return this.page.getByTestId('month-calendar');
  }
  get week() {
    return this.page.getByTestId('week-calendar');
  }
  get agenda() {
    return this.page.getByTestId('agenda-calendar');
  }
  get title() {
    return this.page.getByLabel('Title', { exact: true });
  }
  get cohort() {
    return this.page.getByLabel('Cohort', { exact: true }).last();
  }
  get mentor() {
    return this.page.getByLabel('Mentor', { exact: true });
  }
  get date() {
    return this.page.getByLabel('Date', { exact: true });
  }
  get time() {
    return this.page.getByLabel('Time', { exact: true });
  }
  get location() {
    return this.page.getByLabel('Location', { exact: true });
  }
  get repeatsWeekly() {
    return this.page.getByRole('checkbox', { name: /repeat weekly/i });
  }
  get repeatsWeeklyControl() {
    return this.page.locator('label.w-check').filter({ hasText: /repeat weekly/i });
  }
  get schedule() {
    return this.page.getByRole('button', { name: /schedule session/i });
  }
  get status() {
    return this.page.locator('p.w-sr-only[role="status"]');
  }
}

export class MentorAttendanceCapturePage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { level: 1 });
  }
  get rows() {
    return this.page.getByTestId('mentor-attendance-row');
  }
  row(name: string) {
    return this.rows.filter({ hasText: name });
  }
  statusButton(name: string, status: 'Present' | 'Late' | 'Excused' | 'Absent') {
    return this.row(name).getByRole('button', { name: status, exact: true });
  }
  note(name: string) {
    return this.row(name).getByLabel(`Staff note for ${name}`, { exact: true });
  }
  get allPresent() {
    return this.page.getByRole('button', { name: 'All present', exact: true });
  }
  get save() {
    return this.page.getByRole('button', { name: /save attendance/i });
  }
  get status() {
    return this.page.getByRole('status');
  }
}

export class MentorAttendanceHistoryPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { name: 'Attendance', exact: true });
  }
  get sessionTab() {
    return this.page.getByRole('tab', { name: 'By session', exact: true });
  }
  get youthTab() {
    return this.page.getByRole('tab', { name: 'By youth', exact: true });
  }
  get from() {
    return this.page.getByLabel('From', { exact: true });
  }
  get to() {
    return this.page.getByLabel('To', { exact: true });
  }
  get applyFilters() {
    return this.page.getByRole('button', { name: 'Apply filters', exact: true });
  }
  get sessionRows() {
    return this.page.getByTestId('attendance-session-row');
  }
  get youthRows() {
    return this.page.getByTestId('attendance-youth-row');
  }
  get exportCsv() {
    return this.page.getByRole('button', { name: 'Export CSV', exact: true });
  }
  get exportPdf() {
    return this.page.getByRole('button', { name: 'Export PDF', exact: true });
  }
}
