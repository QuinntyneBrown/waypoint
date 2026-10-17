import { DOCUMENT, DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  HostListener,
  inject,
  OnDestroy,
  signal,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiError, MentorService, SubmissionReviewView } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  WButton,
  WFormField,
  WIcon,
  WLabel,
  WLink,
  WShell,
  WTextarea,
  WTextField,
  WCheckbox,
  WCheckboxBox,
  WAlert,
  WCard,
  WCardHeader,
  WCardTitle,
  WCardBody,
} from '@waypoint/components';
import { catchError, map, Observable, of, tap } from 'rxjs';
import { UnsavedChangesAware, DraftSaveResult, UnsavedChangesService } from '@waypoint/core';

type Decision = 'approve' | 'request-revision' | 'save-draft';

interface GradingDraft {
  scores: Record<string, number | null>;
  comment: string;
  guardianNotified: boolean;
}

// Traces to: L2-MENTOR-003, L2-LEARN-007, L2-ENGAGE-008
@Component({
  selector: 'w-mentor-submission-review',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    WShell,
    RouterLink,
    DatePipe,
    WIcon,
    WButton,
    WLink,
    WFormField,
    WLabel,
    WTextField,
    WTextarea,
    WCheckbox,
    WCheckboxBox,
    WAlert,
    WCard,
    WCardHeader,
    WCardTitle,
    WCardBody,
  ],
  templateUrl: './mentor-submission-review.html',
  styles: `
    .review-layout {
      display: grid;
      grid-template-columns: minmax(0, 1fr) minmax(19rem, 0.75fr);
      gap: var(--w-s6);
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
      gap: var(--w-s4);
      align-items: center;
      padding: var(--w-s4) 0;
      border-bottom: 1px solid var(--w-border);
    }
    .review-actions {
      display: grid;
      gap: var(--w-s3);
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
    }
  `,
})
export class MentorSubmissionReviewPage implements OnDestroy, UnsavedChangesAware {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(MentorService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly document = inject(DOCUMENT);
  private readonly unsavedChanges = inject(UnsavedChangesService);

  private readonly submissionId = this.route.snapshot.paramMap.get('id') ?? '';
  private photoObjectUrl: string | undefined;
  private fileObjectUrl: string | undefined;
  protected readonly review = signal<SubmissionReviewView | null>(null);
  protected readonly photoUrl = signal<string | undefined>(undefined);
  protected readonly fileUrl = signal<string | undefined>(undefined);
  protected readonly scores = signal<Record<string, number | null>>({});
  protected readonly comment = signal('');
  protected readonly guardianNotified = signal(true);
  protected readonly saving = signal(false);
  protected readonly statusMessage = signal<string | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly conflictMessage = signal<string | null>(null);
  private readonly savedDraft = signal<GradingDraft | null>(null);
  protected readonly total = computed(() =>
    Object.values(this.scores()).reduce<number>((sum, score) => sum + (score ?? 0), 0),
  );
  protected readonly dirty = computed(() => {
    const saved = this.savedDraft();
    return saved !== null && JSON.stringify(this.snapshot()) !== JSON.stringify(saved);
  });

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
      .gradeSubmission(this.submissionId, this.gradeRequest(decision, review.version, scoreMap))
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.savedDraft.set(this.snapshot());
          if (decision === 'save-draft') {
            this.load('Draft saved. The youth-visible status has not changed.');
          } else {
            void this.router.navigate(['/mentor/reviews']);
          }
        },
        error: (error: unknown) => {
          this.saving.set(false);
          const apiError = ApiError.from(error);
          if (apiError.status === 409) {
            this.conflictMessage.set(
              'This review changed on the server. Your scores and feedback are still here. Load the latest review to reconcile.',
            );
          } else {
            this.errorMessage.set(apiError.title);
          }
        },
      });
  }

  canDeactivate(): DraftSaveResult {
    return this.unsavedChanges.confirmSaveOrStay(this.dirty(), 'grading', () =>
      this.saveDraftForNavigation(),
    );
  }

  @HostListener('window:beforeunload', ['$event'])
  protected beforeUnload(event: BeforeUnloadEvent): void {
    this.unsavedChanges.warnBeforeUnload(event, this.dirty());
  }

  protected loadLatest(): void {
    this.conflictMessage.set(null);
    this.errorMessage.set(null);
    this.load();
  }

  ngOnDestroy(): void {
    if (this.photoObjectUrl) this.document.defaultView?.URL.revokeObjectURL(this.photoObjectUrl);
    if (this.fileObjectUrl) this.document.defaultView?.URL.revokeObjectURL(this.fileObjectUrl);
  }

  private load(successMessage?: string): void {
    this.api.submission(this.submissionId).subscribe({
      next: (review) => {
        this.review.set(review);
        if (review.photoUrl) this.loadPhoto(review.photoUrl);
        if (review.file) this.loadFile(review.file.url);
        this.scores.set(
          Object.fromEntries(
            review.rubric.map((criterion) => [criterion.criterionId, criterion.awardedPoints]),
          ),
        );
        this.comment.set(review.mentorComment ?? '');
        if (
          review.mentorComment !== null ||
          review.totalAwarded !== null ||
          review.status === 'Approved' ||
          review.status === 'NeedsRevision'
        ) {
          this.guardianNotified.set(review.guardianNotified);
        }
        this.savedDraft.set(this.snapshot());
        this.conflictMessage.set(null);
        if (successMessage) this.statusMessage.set(successMessage);
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  private loadPhoto(path: string): void {
    this.api.submissionFile(path).subscribe({
      next: (blob) => {
        this.photoObjectUrl = this.document.defaultView?.URL.createObjectURL(blob);
        this.photoUrl.set(this.photoObjectUrl);
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  private loadFile(path: string): void {
    this.api.submissionFile(path).subscribe({
      next: (blob) => {
        this.fileObjectUrl = this.document.defaultView?.URL.createObjectURL(blob);
        this.fileUrl.set(this.fileObjectUrl);
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  private saveDraftForNavigation(): Observable<boolean> {
    const review = this.review();
    if (!review || this.saving()) return of(false);
    const scoreMap = Object.fromEntries(
      Object.entries(this.scores()).filter((entry): entry is [string, number] => entry[1] != null),
    );
    this.saving.set(true);
    this.errorMessage.set(null);
    return this.api
      .gradeSubmission(this.submissionId, this.gradeRequest('save-draft', review.version, scoreMap))
      .pipe(
        tap(() => {
          this.saving.set(false);
          this.savedDraft.set(this.snapshot());
          this.statusMessage.set('Draft saved. The youth-visible status has not changed.');
        }),
        map(() => true),
        catchError((error: unknown) => {
          this.saving.set(false);
          this.errorMessage.set(ApiError.from(error).title);
          return of(false);
        }),
      );
  }

  private gradeRequest(decision: Decision, version: string, scoreMap: Record<string, number>) {
    return {
      decision,
      rubricScoresJson: Object.keys(scoreMap).length ? JSON.stringify(scoreMap) : null,
      totalAwarded: this.total(),
      comment: this.comment().trim() || null,
      guardianNotified: this.guardianNotified(),
      expectedVersion: version,
    };
  }

  private snapshot(): GradingDraft {
    return {
      scores: this.scores(),
      comment: this.comment(),
      guardianNotified: this.guardianNotified(),
    };
  }
}
