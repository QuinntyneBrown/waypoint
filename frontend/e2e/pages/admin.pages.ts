import { BasePage } from './base.page';

/** `/admin/people` — the Program Administrator people directory. */
export class PeopleDirectoryPage extends BasePage {
  static readonly path = '/admin/people';
  static readonly urlPattern = /\/admin\/people/;

  get searchInput() {
    return this.page.getByLabel('Search people');
  }
  /** Empty-state heading shown when no people match. */
  get emptyState() {
    return this.page.getByRole('heading', { name: /no people found/i });
  }
  get inviteCta() {
    // An invite CTA appears in the header and (when empty) the empty state.
    return this.page.getByRole('link', { name: /invite/i }).first();
  }
  roleFilter(name: string) {
    return this.page.getByRole('button', { name, exact: true });
  }
  get cohortFilter() {
    return this.page.getByLabel('Cohort');
  }
  get nextPage() {
    return this.page.getByRole('button', { name: 'Next page' });
  }
  get previousPage() {
    return this.page.getByRole('button', { name: 'Previous page' });
  }
  get pageStatus() {
    return this.page.getByTestId('people-page-status');
  }
  /** A directory row identified by a unique cell value (e.g. an email). */
  rowFor(text: string) {
    return this.page.getByText(text, { exact: true });
  }

  async goto(): Promise<void> {
    await this.page.goto(PeopleDirectoryPage.path);
  }

  async search(term: string): Promise<void> {
    await this.searchInput.fill(term);
  }
}

/** `/admin/people/:id` — a person record and its account lifecycle controls. */
export class PersonDetailPage extends BasePage {
  static path(id: string): string {
    return `/admin/people/${id}`;
  }

  get heading() {
    return this.page.getByRole('heading', { level: 1 });
  }
  tab(name: string) {
    return this.page.getByRole('tab', { name, exact: true });
  }
  panel(name: string) {
    return this.page.getByRole('tabpanel', { name, exact: true });
  }
  get outcome() {
    return this.page.getByRole('status');
  }
  get roleSelect() {
    return this.page.getByLabel('Account role');
  }
  get changeRole() {
    return this.page.getByRole('button', { name: 'Change role' });
  }
  get sendPasswordReset() {
    return this.page.getByRole('button', { name: 'Send password reset' });
  }
  get deactivate() {
    return this.page.getByRole('button', { name: 'Deactivate account' });
  }
  get reactivate() {
    return this.page.getByRole('button', { name: 'Reactivate account' });
  }
  get remove() {
    return this.page.getByRole('button', { name: 'Remove person' });
  }
  dialog(name: string | RegExp) {
    return this.page.getByRole('dialog', { name });
  }

  async goto(id: string): Promise<void> {
    await this.page.goto(PersonDetailPage.path(id));
  }
}

/** `/admin/cohorts` — cohort list and creation flow. */
export class CohortsPage extends BasePage {
  static readonly path = '/admin/cohorts';

  get heading() {
    return this.page.getByRole('heading', { name: 'Cohorts', exact: true });
  }
  get createButton() {
    return this.page.getByRole('button', { name: 'New cohort' });
  }
  get createDialog() {
    return this.page.getByRole('dialog', { name: 'Create cohort' });
  }
  get emptyState() {
    return this.page.getByRole('heading', { name: 'No cohorts yet' });
  }
  get retry() {
    return this.page.getByRole('button', { name: 'Retry' });
  }
  card(name: string) {
    return this.page.getByTestId('cohort-card').filter({ hasText: name });
  }

  async goto(): Promise<void> {
    await this.page.goto(CohortsPage.path);
  }
}

/** `/admin/cohorts/:id` — cohort details, editing, and membership. */
export class CohortDetailPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { level: 1 });
  }
  get memberCount() {
    return this.page.getByTestId('cohort-member-count');
  }
  get editButton() {
    return this.page.getByRole('button', { name: 'Edit cohort' });
  }
  get editDialog() {
    return this.page.getByRole('dialog', { name: 'Edit cohort' });
  }
  get youthSelect() {
    return this.page.getByLabel('Add existing youth');
  }
  get addYouth() {
    return this.page.getByRole('button', { name: 'Add youth' });
  }
  get inviteYouth() {
    return this.page.getByRole('link', { name: 'Invite a youth' });
  }
  member(name: string) {
    return this.page.getByTestId('cohort-member').filter({ hasText: name });
  }

  async goto(id: string): Promise<void> {
    await this.page.goto(`/admin/cohorts/${id}`);
  }
}

/** `/admin/cohort-progress` — selected cohort/track progress matrix and exports. */
export class ProgressBoardPage extends BasePage {
  static readonly path = '/admin/cohort-progress';

  get cohortSelect() {
    return this.page.getByLabel('Cohort');
  }
  get trackSelect() {
    return this.page.getByLabel('Track');
  }
  get matrix() {
    return this.page.getByTestId('progress-matrix');
  }
  get liveStatus() {
    return this.page.getByTestId('progress-live-status');
  }
  get emptyState() {
    return this.page.getByRole('heading', { name: 'No published modules' });
  }
  get exportCsv() {
    return this.page.getByRole('button', { name: 'Export CSV' });
  }
  get exportPdf() {
    return this.page.getByRole('button', { name: 'Export PDF' });
  }
  row(name: string) {
    return this.page.getByTestId('progress-row').filter({ hasText: name });
  }

  async goto(): Promise<void> {
    await this.page.goto(ProgressBoardPage.path);
  }
}

/** `/admin/mentor-assignments` — youth pairing, cohort teams, and mentor load. */
export class MentorAssignmentsPage extends BasePage {
  static readonly path = '/admin/mentor-assignments';

  get youthSelect() {
    return this.page.getByLabel('Youth', { exact: true });
  }
  get mentorSelect() {
    return this.page.getByLabel('Assigned mentor');
  }
  get assignMentor() {
    return this.page.getByRole('button', { name: 'Assign mentor' });
  }
  get cohortSelect() {
    return this.page.getByLabel('Cohort team');
  }
  get leadMentorSelect() {
    return this.page.getByLabel('Lead mentor');
  }
  supportingMentor(id: string) {
    return this.page.locator(`input[type="checkbox"][value="${id}"]`);
  }
  get saveCohortTeam() {
    return this.page.getByRole('button', { name: 'Save cohort team' });
  }
  get outcome() {
    return this.page.getByRole('status');
  }
  mentorLoad(id: string) {
    return this.page.locator(`[data-testid="mentor-load-card"][data-mentor-id="${id}"]`);
  }

  async goto(): Promise<void> {
    await this.page.goto(MentorAssignmentsPage.path);
  }
}

/** `/admin/settings` — identity, consent policy, terms, and track availability. */
export class OrganizationSettingsPage extends BasePage {
  static readonly path = '/admin/settings';

  get organizationName() {
    return this.page.getByLabel('Organization name');
  }
  get displayName() {
    return this.page.getByLabel('Display name');
  }
  get logoUrl() {
    return this.page.getByLabel('Logo URL');
  }
  get city() {
    return this.page.getByLabel('City');
  }
  get contactEmail() {
    return this.page.getByLabel('Contact email');
  }
  get consentStatement() {
    return this.page.getByLabel('Consent statement');
  }
  get reconsentEachTerm() {
    return this.page.getByRole('checkbox', { name: 'Require re-consent each term' });
  }
  get requireMediaConsent() {
    return this.page.getByRole('checkbox', { name: 'Require media consent for youth media' });
  }
  get saveSettings() {
    return this.page.getByRole('button', { name: 'Save organization settings' });
  }
  get termName() {
    return this.page.getByLabel('Term name');
  }
  get termStart() {
    return this.page.getByLabel('Start date');
  }
  get termEnd() {
    return this.page.getByLabel('End date');
  }
  get termStatus() {
    return this.page.getByLabel('Term status');
  }
  get saveTerm() {
    return this.page.getByRole('button', { name: 'Create term' });
  }
  get outcome() {
    return this.page.getByTestId('settings-outcome');
  }
  term(name: string) {
    return this.page.getByTestId('term-row').filter({ hasText: name });
  }
  track(id: string) {
    return this.page.locator(`[data-testid="track-row"][data-track-id="${id}"]`);
  }

  async goto(): Promise<void> {
    await this.page.goto(OrganizationSettingsPage.path);
  }
}

/** `/admin/applications` — the Program Administrator application review queue. */
export class ApplicationsListPage extends BasePage {
  static readonly path = '/admin/applications';
  static readonly urlPattern = /\/admin\/applications/;

  get heading() {
    return this.page.getByRole('heading', { name: /applications/i });
  }
  get emptyState() {
    return this.page.getByRole('heading', { name: /no applications/i });
  }
  /**
   * A queue row identified by a unique substring (e.g. an applicant's unique last
   * name); substring match because the applicant cell combines first + last name.
   */
  rowFor(text: string) {
    return this.page.getByTestId('application-row').filter({ hasText: text });
  }
  get statusFilter() {
    return this.page.getByLabel('Application status');
  }
  review(text: string) {
    return this.rowFor(text).getByRole('button', { name: /review/i });
  }
  get reviewDialog() {
    return this.page.getByRole('dialog', { name: /review application/i });
  }
  get trackInterest() {
    return this.page.getByLabel('Interest track');
  }
  get cohort() {
    return this.page.getByLabel('Assign cohort');
  }
  get mentor() {
    return this.page.getByLabel('Assign mentor');
  }
  get decisionNote() {
    return this.page.getByLabel('Decision note');
  }
  get approve() {
    return this.reviewDialog.getByRole('button', { name: /^approve$/i });
  }
  get decline() {
    return this.reviewDialog.getByRole('button', { name: /^decline$/i });
  }

  async goto(): Promise<void> {
    await this.page.goto(ApplicationsListPage.path);
  }
}

/** Complete curriculum authoring workspace. */
export class CurriculumBuilderPage extends BasePage {
  static readonly path = '/admin/tracks';

  get heading() {
    return this.page.getByRole('heading', { name: 'Curriculum builder', exact: true });
  }
  get trackSelect() {
    return this.page.getByLabel('Curriculum track');
  }
  get newTrack() {
    return this.page.getByRole('button', { name: 'New track' });
  }
  get dialogBackdrop() {
    return this.page.locator('.w-modal__scrim');
  }
  get trackDialog() {
    return this.page.getByRole('dialog', { name: 'Create track' });
  }
  get lessonDialog() {
    return this.page.getByRole('dialog', { name: 'Add lesson' });
  }
  get quizDialog() {
    return this.page.getByRole('dialog', { name: 'Add quiz' });
  }
  get assignmentDialog() {
    return this.page.getByRole('dialog', { name: 'Add assignment' });
  }
  get questionEditor() {
    return this.page.getByTestId('quiz-editor');
  }
  get outcome() {
    return this.page.getByTestId('curriculum-outcome');
  }
  module(name: string) {
    return this.page.getByTestId('curriculum-module').filter({ hasText: name });
  }
  lesson(title: string) {
    return this.page.getByTestId('curriculum-lesson').filter({ hasText: title });
  }
  assignment(title: string) {
    return this.page.getByTestId('curriculum-assignment').filter({ hasText: title });
  }
  /** Attaches a streamed video to a lesson card through the chunked upload control. */
  async attachVideo(
    lesson: ReturnType<CurriculumBuilderPage['lesson']>,
    file: { name: string; buffer: Buffer },
  ): Promise<void> {
    await lesson
      .getByLabel('Attach video')
      .setInputFiles({ name: file.name, mimeType: 'video/mp4', buffer: file.buffer });
  }
  /** The row for an upload that has not completed (uploading, interrupted or rejected). */
  videoUpload(lesson: ReturnType<CurriculumBuilderPage['lesson']>, fileName: string) {
    return lesson.getByTestId('lesson-video-upload').filter({ hasText: fileName });
  }
  /** The row for an accepted video and its Queued, Processing, Ready or Failed state. */
  video(lesson: ReturnType<CurriculumBuilderPage['lesson']>, fileName: string) {
    return lesson.getByTestId('lesson-video').filter({ hasText: fileName });
  }
  videoRetryButton(video: ReturnType<CurriculumBuilderPage['video']>) {
    return video.getByRole('button', { name: 'Retry' });
  }
  quiz(id: string) {
    return this.page.locator('[data-testid="curriculum-quiz"][data-quiz-id="' + id + '"]');
  }

  async goto(): Promise<void> {
    await this.page.goto(CurriculumBuilderPage.path);
  }
}

/** Automatic/manual badge definitions and awards. */
export class BadgeBuilderPage extends BasePage {
  static readonly path = '/admin/badges';

  get heading() {
    return this.page.getByRole('heading', { name: 'Badge builder', exact: true });
  }
  get name() {
    return this.page.getByLabel('Badge name');
  }
  get icon() {
    return this.page.getByLabel('Badge icon');
  }
  get criteriaKind() {
    return this.page.getByLabel('Criteria kind');
  }
  get autoRule() {
    return this.page.getByLabel('Automatic rule');
  }
  get target() {
    return this.page.getByLabel('Rule target');
  }
  get competency() {
    return this.page.getByLabel('Mapped competency');
  }
  get saveBadge() {
    return this.page.getByRole('button', { name: 'Save badge' });
  }
  get youth() {
    return this.page.getByLabel('Youth recipient');
  }
  get outcome() {
    return this.page.getByTestId('badge-outcome');
  }
  badge(name: string) {
    return this.page.getByTestId('badge-definition').filter({ hasText: name });
  }

  async goto(): Promise<void> {
    await this.page.goto(BadgeBuilderPage.path);
  }
}

/** Ordered stages and competency framework editor. */
export class ProgressionBuilderPage extends BasePage {
  static readonly path = '/admin/progression';

  get heading() {
    return this.page.getByRole('heading', { name: 'Progression framework', exact: true });
  }
  get addStage() {
    return this.page.getByRole('button', { name: 'Add stage' });
  }
  get stageDialog() {
    return this.page.getByRole('dialog', { name: /stage/i });
  }
  get competencyDialog() {
    return this.page.getByRole('dialog', { name: 'Add competency' });
  }
  get outcome() {
    return this.page.getByTestId('progression-outcome');
  }
  stage(name: string) {
    return this.page.getByTestId('progression-stage').filter({ hasText: name });
  }

  async goto(): Promise<void> {
    await this.page.goto(ProgressionBuilderPage.path);
  }
}

/** Organization-wide session calendar and recurrence editor. */
export class AdminCalendarPage extends BasePage {
  static readonly path = '/admin/calendar';

  get heading() {
    return this.page.getByRole('heading', { name: 'Program calendar', exact: true });
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
  get filter() {
    return this.page.getByLabel('Calendar cohort');
  }
  get title() {
    return this.page.getByLabel('Title', { exact: true });
  }
  get cohort() {
    return this.page.getByLabel('Cohort', { exact: true });
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
    return this.page.getByRole('button', { name: 'Schedule session', exact: true });
  }
  get status() {
    return this.page.locator('p.w-sr-only[role="status"]');
  }
  session(title: string) {
    return this.agenda.getByRole('article').filter({ hasText: title });
  }

  async goto(): Promise<void> {
    await this.page.goto(AdminCalendarPage.path);
  }
}

/** Director attendance capture with four-state marking and staff notes. */
export class AdminAttendanceCapturePage extends BasePage {
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
    return this.row(name).getByLabel('Staff note for ' + name, { exact: true });
  }
  get markedTally() {
    return this.page.getByTestId('mentor-marked-tally');
  }
  get allPresent() {
    return this.page.getByRole('button', { name: 'All present', exact: true });
  }
  get clear() {
    return this.page.getByRole('button', { name: 'Clear', exact: true });
  }
  get save() {
    return this.page.getByRole('button', { name: 'Save attendance', exact: true });
  }
  get status() {
    return this.page.getByRole('status');
  }
  get calendar() {
    return this.page.getByRole('main').getByRole('link', { name: 'Calendar', exact: true });
  }
}

/** Organization-wide attendance history, rates, filters, and exports. */
export class AdminAttendanceHistoryPage extends BasePage {
  static readonly path = '/admin/attendance';

  get heading() {
    return this.page.getByRole('heading', {
      name: 'Attendance administration',
      exact: true,
    });
  }
  get sessionTab() {
    return this.page.getByRole('tab', { name: 'By session', exact: true });
  }
  get youthTab() {
    return this.page.getByRole('tab', { name: 'By youth', exact: true });
  }
  get cohort() {
    return this.page.getByLabel('Cohort', { exact: true });
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

  async goto(): Promise<void> {
    await this.page.goto(AdminAttendanceHistoryPage.path);
  }
}

/** Organization/cohort announcement composition, scheduling, and read statistics. */
export class AdminAnnouncementsPage extends BasePage {
  static readonly path = '/admin/announcements';

  get heading() {
    return this.page.getByRole('heading', { name: 'Announcement administration', exact: true });
  }
  get title() {
    return this.page.getByLabel('Title', { exact: true });
  }
  get message() {
    return this.page.getByLabel('Message', { exact: true });
  }
  get audience() {
    return this.page.getByLabel('Audience', { exact: true });
  }
  get cohort() {
    return this.page.getByLabel('Cohort', { exact: true });
  }
  get recipients() {
    return this.page.getByLabel('Recipients', { exact: true });
  }
  get sendNow() {
    return this.page.getByRole('radio', { name: 'Send now', exact: true });
  }
  get scheduleLater() {
    return this.page.getByRole('radio', { name: 'Schedule for later', exact: true });
  }
  get scheduledAt() {
    return this.page.getByLabel('Send date and time', { exact: true });
  }
  get submit() {
    return this.page.getByRole('button', { name: /send announcement|schedule announcement/i });
  }
  get status() {
    return this.page.getByRole('status');
  }
  get announcements() {
    return this.page.getByTestId('admin-announcement');
  }
  announcement(title: string) {
    return this.announcements.filter({ hasText: title });
  }

  async goto(): Promise<void> {
    await this.page.goto(AdminAnnouncementsPage.path);
  }
}

/** Filtered organization, youth, cohort, attendance, and staff reports. */
export class AdminReportsPage extends BasePage {
  static readonly path = '/admin/reports';

  get heading() {
    return this.page.getByRole('heading', { name: 'Reports hub', exact: true });
  }
  get from() {
    return this.page.getByLabel('Date from', { exact: true });
  }
  get to() {
    return this.page.getByLabel('Date to', { exact: true });
  }
  get cohortFilter() {
    return this.page.getByLabel('Report cohort', { exact: true });
  }
  get trackFilter() {
    return this.page.getByLabel('Report track', { exact: true });
  }
  get mentorFilter() {
    return this.page.getByLabel('Report mentor', { exact: true });
  }
  get applyFilters() {
    return this.page.getByRole('button', { name: 'Apply report filters', exact: true });
  }
  get updatedAt() {
    return this.page.getByTestId('report-updated-at');
  }
  get recordCount() {
    return this.page.getByTestId('report-record-count');
  }
  get exportCsv() {
    return this.page.getByRole('button', { name: 'Export current report as CSV', exact: true });
  }
  get exportPdf() {
    return this.page.getByRole('button', { name: 'Export current report as PDF', exact: true });
  }
  get overviewTab() {
    return this.page.getByRole('tab', { name: 'Overview', exact: true });
  }
  get youthTab() {
    return this.page.getByRole('tab', { name: 'Youth report', exact: true });
  }
  get cohortTab() {
    return this.page.getByRole('tab', { name: 'Cohort report', exact: true });
  }
  get staffTab() {
    return this.page.getByRole('tab', { name: 'Staff activity', exact: true });
  }
  get overview() {
    return this.page.getByTestId('reports-overview');
  }
  get youth() {
    return this.page.getByLabel('Youth', { exact: true });
  }
  get generateYouth() {
    return this.page.getByRole('button', { name: 'Generate youth report', exact: true });
  }
  get youthReport() {
    return this.page.getByTestId('youth-report');
  }
  get reportCohort() {
    return this.page.getByLabel('Cohort report selection', { exact: true });
  }
  get compareCohort() {
    return this.page.getByLabel('Compare with', { exact: true });
  }
  get generateCohort() {
    return this.page.getByRole('button', { name: 'Generate cohort report', exact: true });
  }
  get cohortReport() {
    return this.page.getByTestId('cohort-report');
  }
  get staffRows() {
    return this.page.getByTestId('staff-activity-row');
  }
  staffRow(name: string) {
    return this.staffRows.filter({ hasText: name });
  }

  async goto(): Promise<void> {
    await this.page.goto(AdminReportsPage.path);
  }
}

/** Three-step CSV roster upload, mapping, validation preview, and ready-row import. */
export class AdminRosterImportPage extends BasePage {
  static readonly path = '/admin/import';

  get heading() {
    return this.page.getByRole('heading', { name: 'Import roster', exact: true });
  }
  get fileInput() {
    return this.page.getByLabel('Roster CSV', { exact: true });
  }
  get fileSummary() {
    return this.page.getByTestId('import-file-summary');
  }
  get fileError() {
    return this.page.getByRole('alert');
  }
  mapColumn(header: string) {
    return this.page.getByLabel(`Map ${header}`, { exact: true });
  }
  get previewButton() {
    return this.page.getByRole('button', { name: 'Preview roster', exact: true });
  }
  get previewSummary() {
    return this.page.getByTestId('import-preview-summary');
  }
  previewRow(text: string) {
    return this.page.getByTestId('import-preview-row').filter({ hasText: text });
  }
  get importReady() {
    return this.page.getByRole('button', { name: /Import \d+ ready/ });
  }
  get confirmation() {
    return this.page.getByRole('dialog', { name: 'Confirm roster import' });
  }
  get confirmImport() {
    return this.page.getByRole('button', { name: 'Confirm import', exact: true });
  }
  get result() {
    return this.page.getByTestId('import-result');
  }

  async goto(): Promise<void> {
    await this.page.goto(AdminRosterImportPage.path);
  }
}

/** Program-administrator audit inspection, filtering, paging, and export. */
export class AdminAuditLogPage extends BasePage {
  static readonly path = '/admin/audit';

  get heading() {
    return this.page.getByRole('heading', { name: 'Audit log', exact: true });
  }
  get userFilter() {
    return this.page.getByLabel('Audit user', { exact: true });
  }
  get actionFilter() {
    return this.page.getByLabel('Audit action', { exact: true });
  }
  get from() {
    return this.page.getByLabel('Audit date from', { exact: true });
  }
  get to() {
    return this.page.getByLabel('Audit date to', { exact: true });
  }
  get applyFilters() {
    return this.page.getByRole('button', { name: 'Apply audit filters', exact: true });
  }
  get clearFilters() {
    return this.page.getByRole('button', { name: 'Clear audit filters', exact: true }).first();
  }
  get exportCsv() {
    return this.page.getByRole('button', { name: 'Export filtered audit log as CSV', exact: true });
  }
  get rows() {
    return this.page.getByTestId('audit-row');
  }
  row(text: string) {
    return this.rows.filter({ hasText: text });
  }
  get summary() {
    return this.page.getByTestId('audit-page-summary');
  }
  get previousPage() {
    return this.page.getByRole('button', { name: 'Previous audit page', exact: true });
  }
  get nextPage() {
    return this.page.getByRole('button', { name: 'Next audit page', exact: true });
  }

  async goto(): Promise<void> {
    await this.page.goto(AdminAuditLogPage.path);
  }
}

/** `/admin/dashboard` — the report-backed director home. */
export class AdminDashboardPage extends BasePage {
  static readonly path = '/admin/dashboard';
  get heading() {
    return this.page.getByRole('heading', { name: /welcome back/i });
  }
  get summary() {
    return this.page.getByLabel('Program summary');
  }
  async goto(): Promise<void> {
    await this.page.goto(AdminDashboardPage.path);
  }
}

/** Shared `/admin/invitations` management surface. */
export class InvitationsPage extends BasePage {
  static readonly path = '/admin/invitations';
  get emails() {
    return this.page.getByLabel('Email addresses');
  }
  get sendButton() {
    return this.page.getByRole('button', { name: /send invitation/i });
  }
  get rows() {
    return this.page.getByTestId('invitation-row');
  }
  async goto(path = InvitationsPage.path): Promise<void> {
    await this.page.goto(path);
  }
}

/** `/admin/privacy` — administrator data-subject request queue. */
export class AdminPrivacyPage extends BasePage {
  static readonly path = '/admin/privacy';
  get heading() {
    return this.page.getByRole('heading', { name: 'Privacy requests', exact: true });
  }
  async goto(): Promise<void> {
    await this.page.goto(AdminPrivacyPage.path);
  }
}
