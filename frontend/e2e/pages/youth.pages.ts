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
