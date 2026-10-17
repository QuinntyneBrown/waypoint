import { DOCUMENT, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiError, MentorService, QuizAssessmentReview } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WButton, WShell } from '@waypoint/components';

// Traces to: L2-MENTOR-004, L2-LEARN-005
@Component({
  selector: 'w-mentor-quiz-review',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, RouterLink, DatePipe, WButton],
  templateUrl: './mentor-quiz-review.html',
  styles: `
    .assessment-layout {
      display: grid;
      grid-template-columns: minmax(0, 1fr) minmax(18rem, 0.65fr);
      gap: var(--w-s6);
      align-items: start;
    }
    .score-grid {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: var(--w-s4);
    }
    .answer-list {
      list-style: none;
      padding: 0;
      margin: 0;
      display: grid;
      gap: var(--w-s3);
    }
    .answer-copy {
      min-width: 0;
      flex: 1;
      overflow-wrap: anywhere;
    }
    .approve-button {
      min-height: 44px;
    }
    @media (max-width: 52rem) {
      .assessment-layout {
        grid-template-columns: 1fr;
      }
    }
    @media (max-width: 32rem) {
      .score-grid {
        grid-template-columns: 1fr;
      }
    }
  `,
})
export class MentorQuizReviewPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(MentorService);
  private readonly route = inject(ActivatedRoute);
  private readonly document = inject(DOCUMENT);

  private readonly attemptId = this.route.snapshot.paramMap.get('id') ?? '';
  protected readonly review = signal<QuizAssessmentReview | null>(null);
  protected readonly overrideScore = signal('');
  protected readonly approving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly statusMessage = signal<string | null>(null);

  constructor() {
    this.load();
  }

  protected setOverrideScore(value: string): void {
    this.overrideScore.set(value);
    this.errorMessage.set(null);
  }

  protected approve(): void {
    const review = this.review();
    if (!review || review.approvedScore != null || this.approving()) return;
    const raw = this.overrideScore().trim();
    const override = raw === '' ? null : Number(raw);
    if (override != null && (!Number.isInteger(override) || override < 0 || override > 100)) {
      this.errorMessage.set('Override score must be a whole number between 0 and 100.');
      return;
    }
    const score = override ?? review.autoScore;
    if (
      !this.document.defaultView?.confirm(`Approve ${score}% as the score this youth will see?`)
    ) {
      return;
    }

    this.approving.set(true);
    this.errorMessage.set(null);
    this.api.approveQuizAttempt(this.attemptId, override).subscribe({
      next: () => {
        this.approving.set(false);
        this.statusMessage.set('Quiz assessment approved.');
        this.load();
      },
      error: (error: unknown) => {
        this.approving.set(false);
        this.errorMessage.set(ApiError.from(error).title);
      },
    });
  }

  protected optionSelected(
    question: QuizAssessmentReview['questions'][number],
    index: number,
  ): boolean {
    return question.selectedIndexes.includes(index);
  }

  protected optionCorrect(
    question: QuizAssessmentReview['questions'][number],
    index: number,
  ): boolean {
    return question.correctIndexes.includes(index);
  }

  private load(): void {
    this.api.quizAssessment(this.attemptId).subscribe({
      next: (review) => this.review.set(review),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }
}
