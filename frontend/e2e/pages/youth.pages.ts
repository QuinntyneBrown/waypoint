import { BasePage } from './base.page';
import { AppShell } from './shell';

/** `/youth/home` — the youth landing surface inside the app shell. */
export class YouthHomePage extends BasePage {
  static readonly path = '/youth/home';
  static readonly urlPattern = /\/youth\/home/;

  /** The surrounding app shell (sign-out, offline banner, bottom nav). */
  get shell() {
    return new AppShell(this.page);
  }

  get welcomeHeading() {
    return this.page.getByRole('heading', { name: /welcome/i });
  }
  /** Empty-state heading shown when the youth is not placed in a cohort. */
  get noCohortHeading() {
    return this.page.getByRole('heading', { name: /not in a cohort yet/i });
  }
  get messageMentorLink() {
    return this.page.getByRole('link', { name: /message your mentor/i });
  }
  get placedDashboard() {
    return this.page.getByTestId('placed-home');
  }
  get progressCard() {
    return this.page.getByTestId('home-progress');
  }
  get sessionCard() {
    return this.page.getByTestId('home-session');
  }
  get assignmentsCard() {
    return this.page.getByTestId('home-assignments');
  }
  get continueLearningLink() {
    return this.page.getByRole('link', { name: /start learning|resume learning/i });
  }

  async goto(): Promise<void> {
    await this.page.goto(YouthHomePage.path);
  }
}

/** `/youth/announcements` — the youth announcements feed. */
export class YouthAnnouncementsPage extends BasePage {
  static readonly path = '/youth/announcements';

  get emptyStateHeading() {
    return this.page.getByRole('heading', { name: /no announcements yet/i });
  }

  async goto(): Promise<void> {
    await this.page.goto(YouthAnnouncementsPage.path);
  }
}

/** `/youth/notifications` — the youth notifications feed. */
export class YouthNotificationsPage extends BasePage {
  static readonly path = '/youth/notifications';

  get emptyStateHeading() {
    return this.page.getByRole('heading', { name: /all caught up/i });
  }
  get realtimeStatus() {
    return this.page.getByTestId('notification-realtime-status');
  }
  get items() {
    return this.page.getByTestId('notification-item');
  }
  notification(title: string) {
    return this.items.filter({ has: this.page.getByRole('heading', { name: title, exact: true }) });
  }

  async goto(): Promise<void> {
    await this.page.goto(YouthNotificationsPage.path);
  }
}

/** `/youth/curriculum` — enrolled track switcher and ordered gated module map. */
export class YouthCurriculumPage extends BasePage {
  static readonly path = '/youth/curriculum';

  get heading() {
    return this.page.getByRole('heading', { name: /curriculum/i });
  }
  get trackOptions() {
    return this.page.locator('[data-testid="curriculum-track"]');
  }
  get modules() {
    return this.page.locator('[data-testid="curriculum-module"]');
  }
  get emptyState() {
    return this.page.getByRole('heading', { name: /not set up yet/i });
  }

  module(name: string) {
    return this.modules.filter({
      has: this.page.getByRole('heading', { name, exact: true }),
    });
  }

  async goto(): Promise<void> {
    await this.page.goto(YouthCurriculumPage.path);
  }
}

export class YouthJourneyPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { name: 'My Journey', exact: true });
  }
  get currentModule() {
    return this.page.getByTestId('current-module');
  }
  get badgeDetailsLink() {
    return this.page.getByRole('link', { name: /view badge details/i });
  }
}

export class YouthBadgesPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { name: 'Badges', exact: true });
  }
  get progressLabels() {
    return this.page.getByTestId('badge-progress');
  }
}

export class YouthProgressPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { name: 'My progress', exact: true });
  }
  get overviewTab() {
    return this.page.getByRole('tab', { name: 'Overview', exact: true });
  }
  get quizzesTab() {
    return this.page.getByRole('tab', { name: 'Quizzes', exact: true });
  }
  get attendanceTab() {
    return this.page.getByRole('tab', { name: 'Attendance', exact: true });
  }
  get overviewPanel() {
    return this.page.getByTestId('progress-overview');
  }
  get quizzesPanel() {
    return this.page.getByTestId('progress-quizzes');
  }
  get attendancePanel() {
    return this.page.getByTestId('progress-attendance');
  }
}

/** `/youth/schedule` — upcoming cohort sessions and the youth's current RSVP. */
export class YouthSchedulePage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { name: 'My schedule', exact: true });
  }
  get sessions() {
    return this.page.getByTestId('youth-session');
  }
  get going() {
    return this.page.getByRole('radio', { name: 'Going', exact: true });
  }
  get goingOption() {
    return this.page.locator('label.rsvp-option').filter({ hasText: /^Going$/ });
  }
  get maybe() {
    return this.page.getByRole('radio', { name: 'Maybe', exact: true });
  }
  get maybeOption() {
    return this.page.locator('label.rsvp-option').filter({ hasText: /^Maybe$/ });
  }
  get cantMakeIt() {
    return this.page.getByRole('radio', { name: 'Can’t make it', exact: true });
  }
  get cantMakeItOption() {
    return this.page.locator('label.rsvp-option').filter({ hasText: /^Can’t make it$/ });
  }
  get mentorNote() {
    return this.page.getByLabel(/note to your mentor/i);
  }
  get reminder() {
    return this.page.getByRole('checkbox', { name: /remind me before this session/i });
  }
  get reminderOption() {
    return this.page
      .locator('label.wu-check')
      .filter({ hasText: /remind me before this session/i });
  }
  get saveButton() {
    return this.page.getByRole('button', { name: /save rsvp/i });
  }
  get status() {
    return this.page.locator('[data-testid^="rsvp-status-"]');
  }
  get currentResponse() {
    return this.page.getByTestId('current-rsvp');
  }
  get reminderTime() {
    return this.page.getByTestId('reminder-time');
  }
}

/** `/youth/messages` — the assigned mentor thread with live message delivery. */
export class YouthMessagesPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { name: 'Messages', exact: true });
  }
  get thread() {
    return this.page.getByTestId('message-thread');
  }
  get composer() {
    return this.page.getByLabel(/message your mentor/i);
  }
  get sendButton() {
    return this.page.getByRole('button', { name: /send message/i });
  }
  get realtimeStatus() {
    return this.page.getByTestId('realtime-status');
  }
}

/** `/youth/lessons/:id` — safe lesson reader with persisted reading controls. */
export class YouthLessonPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { level: 1 });
  }
  get stepProgress() {
    return this.page.getByTestId('lesson-step-progress');
  }
  get largeTextButton() {
    return this.page.getByRole('button', { name: /large text/i });
  }
  get bookmarkButton() {
    return this.page.getByRole('button', { name: /bookmark lesson/i });
  }
  get retryMediaButton() {
    return this.page.getByRole('button', { name: /retry media/i });
  }
  get completeButton() {
    return this.page.getByRole('button', { name: /mark lesson complete/i });
  }
}

export class YouthCodingPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { level: 1 });
  }
  get editor() {
    return this.page.getByTestId('coding-editor');
  }
  get preview() {
    return this.page.getByTestId('coding-preview');
  }
  get progress() {
    return this.page.getByTestId('coding-progress');
  }
  get mentorNote() {
    return this.page.getByLabel(/note to your mentor/i);
  }
  get attestation() {
    return this.page.getByRole('checkbox', { name: /this is my own work/i });
  }
  get submitButton() {
    return this.page.getByRole('button', { name: /submit coding project/i });
  }
  step(name: string) {
    return this.page.getByRole('button', { name: new RegExp(name, 'i') });
  }
}

export class YouthReflectionPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { level: 1 });
  }
  get reflectionText() {
    return this.page.getByLabel(/your reflection/i);
  }
  get optionalResponse() {
    return this.page.getByLabel(/one action i will take/i);
  }
  get shareWithMentor() {
    return this.page.getByRole('checkbox', { name: /share with my mentor/i });
  }
  get saveDraftButton() {
    return this.page.getByRole('button', { name: /save draft/i });
  }
  get submitButton() {
    return this.page.getByRole('button', { name: /submit reflection/i });
  }
  get backToLessonLink() {
    return this.page.getByRole('link', { name: /back to lesson/i });
  }
}

export class YouthQuizIntroPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { level: 1 });
  }
  get startButton() {
    return this.page.getByRole('button', { name: /start attempt|resume attempt/i });
  }
}

export class YouthQuizAttemptPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { level: 1 });
  }
  get questionHeading() {
    return this.page.getByRole('heading', { level: 2 });
  }
  get progress() {
    return this.page.getByTestId('quiz-progress');
  }
  get nextButton() {
    return this.page.getByRole('button', { name: /next question/i });
  }
  get submitButton() {
    return this.page.getByRole('button', { name: /submit quiz/i });
  }
}

export class YouthQuizResultPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { level: 1 });
  }
  get retakeButton() {
    return this.page.getByRole('button', { name: /retake quiz/i });
  }
}

export class YouthAssignmentPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { level: 1 });
  }
  get responseText() {
    return this.page.getByLabel(/written response/i);
  }
  get evidenceLink() {
    return this.page.getByLabel(/evidence link/i);
  }
  get photoInput() {
    return this.page.getByLabel(/evidence photo/i);
  }
  get attestation() {
    return this.page.getByRole('checkbox', { name: /this is my own work/i });
  }
  get submitButton() {
    return this.page.getByRole('button', { name: /submit assignment/i });
  }
}

export class YouthSubmissionStatusPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { level: 1 });
  }
  get status() {
    return this.page.getByTestId('submission-status');
  }
  get reviseLink() {
    return this.page.getByRole('link', { name: /revise and resubmit/i });
  }
}

export class YouthResubmitPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { level: 1 });
  }
  get responseText() {
    return this.page.getByLabel(/revised response/i);
  }
  get submitButton() {
    return this.page.getByRole('button', { name: /resubmit assignment/i });
  }
}
