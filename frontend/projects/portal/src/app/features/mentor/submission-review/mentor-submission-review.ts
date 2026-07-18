import { DOCUMENT, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiError, MentorApi, SubmissionReviewView } from '@wu/api';
import { AuthStore } from '@wu/auth';
import { WuShell } from '@wu/ui';

type Decision = 'approve' | 'request-revision' | 'save-draft';

// Traces to: L2-MENTOR-003, L2-LEARN-007, L2-ENGAGE-008
@Component({
  selector: 'wu-mentor-submission-review',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, RouterLink, DatePipe],
  templateUrl: './mentor-submission-review.html',
  styles: `
    .review-layout {
      display: grid;
      grid-template-columns: minmax(0, 1fr) minmax(19rem, 0.75fr);
      gap: var(--wu-s6);
      align-items: start;
    }
    .submitted-body {
      white-space: pre-wrap;
      overflow-wrap: anywhere;
    }
    .evidence-image {
      display: block;
      max-width: 100%;
      max-height: 28rem;
      object-fit: contain;
    }
    .rubric-row {
      display: grid;
      grid-template-columns: minmax(0, 1fr) 7rem;
      gap: var(--wu-s4);
      align-items: center;
      padding: var(--wu-s4) 0;
      border-bottom: 1px solid var(--wu-border);
    }
    .review-actions .wu-btn {
      min-height: 44px;
    }
    @media (max-width: 52rem) {
      .review-layout {
        grid-template-columns: 1fr;
      }
    }
    @media (max-width: 32rem) {
      .rubric-row {
        grid-template-columns: 1fr;
      }
      .review-actions {
        display: grid;
        grid-template-columns: 1fr;
      }
    }
  `,
})
export class MentorSubmissionReviewPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(MentorApi);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly document = inject(DOCUMENT);

  private readonly submissionId = this.route.snapshot.paramMap.get('id') ?? '';
  protected readonly review = signal<SubmissionReviewView | null>(null);
  protected readonly scores = signal<Record<string, number | null>>({});
  protected readonly comment = signal('');
  protected readonly guardianNotified = signal(true);
  protected readonly saving = signal(false);
  protected readonly statusMessage = signal<string | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly total = computed(() =>
    Object.values(this.scores()).reduce<number>((sum, score) => sum + (score ?? 0), 0),
  );

  constructor() {
    this.load();
  }

  protected setScore(criterionId: string, rawValue: string): void {
    const value = rawValue === '' ? null : Number(rawValue);
    this.scores.update((scores) => ({ ...scores, [criterionId]: value }));
    this.statusMessage.set(null);
  }

  protected setComment(value: string): void {
    this.comment.set(value);
    this.errorMessage.set(null);
  }

  protected submit(decision: Decision): void {
    const review = this.review();
    if (!review || this.saving()) return;
    if (decision === 'request-revision' && !this.comment().trim()) {
      this.errorMessage.set('Feedback is required when requesting a revision.');
      return;
    }
    if (
      decision === 'approve' &&
      review.rubric.some((criterion) => this.scores()[criterion.criterionId] == null)
    ) {
      this.errorMessage.set('Score every rubric criterion before approving.');
      return;
    }
    if (
      decision !== 'save-draft' &&
      !this.document.defaultView?.confirm(
        decision === 'approve'
          ? 'Approve this submission and notify the youth?'
          : 'Request a revision and notify the youth?',
      )
    ) {
      return;
    }

    const scoreMap = Object.fromEntries(
      Object.entries(this.scores()).filter((entry): entry is [string, number] => entry[1] != null),
    );
    this.saving.set(true);
    this.errorMessage.set(null);
    this.api
      .gradeSubmission(this.submissionId, {
        decision,
        rubricScoresJson: Object.keys(scoreMap).length ? JSON.stringify(scoreMap) : null,
        totalAwarded: this.total(),
        comment: this.comment().trim() || null,
        guardianNotified: this.guardianNotified(),
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          if (decision === 'save-draft') {
            this.statusMessage.set('Draft saved. The youth-visible status has not changed.');
          } else {
            void this.router.navigate(['/mentor/reviews']);
          }
        },
        error: (error: unknown) => {
          this.saving.set(false);
          this.errorMessage.set(ApiError.from(error).title);
        },
      });
  }

  private load(): void {
    this.api.submission(this.submissionId).subscribe({
      next: (review) => {
        this.review.set(review);
        this.scores.set(
          Object.fromEntries(
            review.rubric.map((criterion) => [criterion.criterionId, criterion.awardedPoints]),
          ),
        );
        this.comment.set(review.mentorComment ?? '');
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }
}
