import { BasePage } from './base.page';

/** `/guardian/home` — read-only linked-child summary. */
export class GuardianHomePage extends BasePage {
  static readonly path = '/guardian/home';
  static readonly urlPattern = /\/guardian\/home/;

  get heading() {
    return this.page.getByRole('heading', { name: /your youth at a glance/i });
  }
  get childName() {
    return this.page.getByTestId('guardian-child-name');
  }
  get overallProgress() {
    return this.page.getByTestId('guardian-overall-progress');
  }
  get progressLink() {
    return this.page.getByRole('link', { name: /view full progress/i });
  }
}

/** `/guardian/progress` — modules, attendance, badges, schedule, and shared check-ins. */
export class GuardianProgressPage extends BasePage {
  static readonly path = '/guardian/progress';
  static readonly urlPattern = /\/guardian\/progress/;

  get heading() {
    return this.page.getByRole('heading', { name: /child progress/i });
  }
  get moduleRows() {
    return this.page.locator('[data-testid="guardian-module"]');
  }
  get attendanceRows() {
    return this.page.locator('[data-testid="guardian-attendance"]');
  }
  get quizOutcomes() {
    return this.page.getByTestId('guardian-quiz-outcomes');
  }
  get competencies() {
    return this.page.getByTestId('guardian-competencies');
  }
  get earnedBadges() {
    return this.page.getByTestId('guardian-earned-badges');
  }
  get lockedBadges() {
    return this.page.getByTestId('guardian-locked-badges');
  }
  get upcomingSessions() {
    return this.page.getByTestId('guardian-upcoming-sessions');
  }
  get sharedCheckIns() {
    return this.page.getByTestId('guardian-shared-checkins');
  }
}

/** `/guardian/announcements` — targeted guardian/cohort news feed. */
export class GuardianAnnouncementsPage extends BasePage {
  static readonly path = '/guardian/announcements';
  static readonly urlPattern = /\/guardian\/announcements/;

  get heading() {
    return this.page.getByRole('heading', { name: /news and announcements/i });
  }
  get announcements() {
    return this.page.locator('[data-testid="guardian-announcement"]');
  }
  get unreadAnnouncements() {
    return this.announcements.filter({ has: this.page.getByText('Unread', { exact: true }) });
  }
  get markAllReadButton() {
    return this.page.getByRole('button', { name: /mark all read/i });
  }

  async goto(): Promise<void> {
    await this.page.goto(GuardianAnnouncementsPage.path);
  }
}

/** `/guardian/consent` — contact details, media preferences, and e-sign record. */
export class GuardianConsentPage extends BasePage {
  static readonly path = '/guardian/consent';
  static readonly urlPattern = /\/guardian\/consent/;

  get heading() {
    return this.page.getByRole('heading', { name: /consent and contact/i });
  }
  get participationStatus() {
    return this.page.getByTestId('participation-status');
  }
  get referenceId() {
    return this.page.getByTestId('consent-reference');
  }
  get mediaConsent() {
    return this.page.getByLabel(/allow media use/i);
  }
  get mediaConsentOption() {
    return this.page.getByText('Allow media use', { exact: true });
  }
  get groupMedia() {
    return this.page.getByLabel(/group photos/i);
  }
  get groupMediaOption() {
    return this.page.getByText('Group photos only', { exact: true });
  }
  get firstNameCaptions() {
    return this.page.getByLabel(/first-name captions/i);
  }
  get firstNameCaptionsOption() {
    return this.page.getByText('First-name captions', { exact: true });
  }
  get saveMediaButton() {
    return this.page.getByRole('button', { name: /save media consent/i });
  }
  get phone() {
    return this.page.getByLabel(/^phone$/i);
  }
  get emergencyName() {
    return this.page.getByLabel(/emergency contact name/i);
  }
  get saveContactButton() {
    return this.page.getByRole('button', { name: /save contact details/i });
  }
  get emailCopyButton() {
    return this.page.getByRole('button', { name: /email me a copy/i });
  }
  get withdrawButton() {
    return this.page.getByRole('button', { name: /withdraw consent/i });
  }
  get status() {
    return this.page.getByRole('status');
  }

  async goto(): Promise<void> {
    await this.page.goto(GuardianConsentPage.path);
  }
}
