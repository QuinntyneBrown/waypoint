import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  ApiError,
  ApplicationDetail,
  ApplicationReviewOptions,
  ApplicationsService,
  ApplicationSummary,
} from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  Button,
  ErrorText,
  FormField,
  Icon,
  IconButton,
  Label,
  LabelRequired,
  Select,
  Shell,
  Textarea,
  TextField,
  Alert,
  AlertTitle,
  Badge,
  EmptyState,
  EmptyStateIcon,
  EmptyStateTitle,
  EmptyStateText,
  TableWrap,
  Table,
  Dialog,
  DialogHeader,
  DialogTitle,
  DialogSubtitle,
  DialogBody,
  DialogFooter,
} from '@waypoint/components';
import { Observable } from 'rxjs';

type Decision = 'approve' | 'decline';

// Traces to: L2-INTAKE-002, L2-CONSENT-002, L2-AUTH-009
@Component({
  selector: 'w-applications-list',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    Shell,
    Icon,
    DatePipe,
    ReactiveFormsModule,
    Button,
    IconButton,
    FormField,
    Label,
    LabelRequired,
    ErrorText,
    TextField,
    Textarea,
    Select,
    Alert,
    AlertTitle,
    Badge,
    EmptyState,
    EmptyStateIcon,
    EmptyStateTitle,
    EmptyStateText,
    TableWrap,
    Table,
    Dialog,
    DialogHeader,
    DialogTitle,
    DialogSubtitle,
    DialogBody,
    DialogFooter,
  ],
  templateUrl: './applications-list.html',
  styles: `
    .application-name {
      min-width: 12rem;
    }
    .review-detail {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: var(--w-s4);
    }
    .review-detail__wide {
      grid-column: 1 / -1;
    }
    .detail-copy {
      white-space: pre-wrap;
      overflow-wrap: anywhere;
    }
    .w-modal__footer .w-btn {
      min-height: 45px;
    }
    @media (max-width: 42rem) {
      .review-detail {
        grid-template-columns: 1fr;
      }
      .review-detail__wide {
        grid-column: auto;
      }
      .w-modal__footer .w-btn {
        flex: 1 1 100%;
      }
    }
  `,
})
export class ApplicationsList {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(ApplicationsService);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly applications = signal<ApplicationSummary[] | undefined>(undefined);
  protected readonly options = signal<ApplicationReviewOptions | undefined>(undefined);
  protected readonly reviewing = signal<ApplicationDetail | undefined>(undefined);
  protected readonly loadingReview = signal(false);
  protected readonly decision = signal<Decision | null>(null);
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly outcome = signal<string | null>(null);
  protected readonly selectedCohortId = signal('');
  protected readonly loading = computed(() => this.applications() === undefined);
  protected readonly statusFilter = this.fb.nonNullable.control('');
  protected readonly form = this.fb.nonNullable.group({
    trackInterest: ['', Validators.maxLength(200)],
    cohortId: ['', Validators.required],
    mentorId: ['', Validators.required],
    decisionNote: ['', Validators.maxLength(2000)],
  });
  protected readonly eligibleMentors = computed(() => {
    const cohortId = this.selectedCohortId();
    return (this.options()?.mentors ?? []).filter(
      (mentor) => !cohortId || mentor.cohortIds.includes(cohortId),
    );
  });

  constructor() {
    this.load();
    this.api.options().subscribe({
      next: (options) => this.options.set(options),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
    this.statusFilter.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((status) => this.load(status));
    this.form.controls.cohortId.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((cohortId) => {
        this.selectedCohortId.set(cohortId);
        const mentorId = this.form.controls.mentorId.value;
        if (
          mentorId &&
          !this.options()
            ?.mentors.find((mentor) => mentor.id === mentorId)
            ?.cohortIds.includes(cohortId)
        ) {
          this.form.controls.mentorId.reset();
        }
      });
  }

  protected openReview(summary: ApplicationSummary): void {
    this.loadingReview.set(true);
    this.errorMessage.set(null);
    this.api.detail(summary.id).subscribe({
      next: (detail) => {
        this.reviewing.set(detail);
        this.form.reset({
          trackInterest: detail.trackInterest ?? '',
          cohortId: detail.cohortId ?? '',
          mentorId: detail.mentorId ?? '',
          decisionNote: detail.decisionNote ?? '',
        });
        this.selectedCohortId.set(detail.cohortId ?? '');
        this.loadingReview.set(false);
      },
      error: (error: unknown) => {
        this.loadingReview.set(false);
        this.errorMessage.set(ApiError.from(error).title);
      },
    });
  }

  protected closeReview(): void {
    if (this.saving()) return;
    this.decision.set(null);
    this.reviewing.set(undefined);
  }

  protected reviewApproval(): void {
    if (!this.reviewing()?.canApprove) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.decision.set('approve');
  }

  protected reviewDecline(): void {
    this.decision.set('decline');
  }

  protected cancelDecision(): void {
    if (!this.saving()) this.decision.set(null);
  }

  protected confirmDecision(): void {
    const application = this.reviewing();
    const decision = this.decision();
    if (!application || !decision || this.saving()) return;
    const values = this.form.getRawValue();
    this.saving.set(true);
    this.errorMessage.set(null);
    const request: Observable<unknown> =
      decision === 'approve'
        ? this.api.approve(application.id, {
            cohortId: values.cohortId,
            mentorId: values.mentorId,
            trackInterest: values.trackInterest.trim() || null,
            decisionNote: values.decisionNote.trim() || null,
          })
        : this.api.decline(application.id, values.decisionNote.trim() || null);
    request.subscribe({
      next: () => {
        const name = application.firstName + ' ' + application.lastName;
        this.outcome.set(
          decision === 'approve'
            ? name + ' was approved and enrolled.'
            : name + "'s application was declined.",
        );
        this.saving.set(false);
        this.decision.set(null);
        this.reviewing.set(undefined);
        this.load(this.statusFilter.value, false);
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.decision.set(null);
        this.errorMessage.set(ApiError.from(error).title);
      },
    });
  }

  protected selectedCohortName(): string {
    const id = this.form.controls.cohortId.value;
    return this.options()?.cohorts.find((item) => item.id === id)?.name ?? 'the selected cohort';
  }

  protected selectedMentorName(): string {
    const id = this.form.controls.mentorId.value;
    return this.options()?.mentors.find((item) => item.id === id)?.name ?? 'the selected mentor';
  }

  private load(status = this.statusFilter.value, showLoading = true): void {
    if (showLoading) this.applications.set(undefined);
    this.api.list(status || null).subscribe({
      next: (items) => this.applications.set(items),
      error: (error: unknown) => {
        this.applications.set([]);
        this.errorMessage.set(ApiError.from(error).title);
      },
    });
  }
}
