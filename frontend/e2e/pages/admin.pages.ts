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
