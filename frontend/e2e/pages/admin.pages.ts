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
    return this.page.getByText(text);
  }

  async goto(): Promise<void> {
    await this.page.goto(ApplicationsListPage.path);
  }
}
