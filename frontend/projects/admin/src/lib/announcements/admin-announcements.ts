import { DatePipe, PercentPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  AnnouncementRoleTarget,
  AnnouncementsService,
  ApiError,
  CohortsService,
  CohortSummary,
  ManagedAnnouncement,
} from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  WButton,
  WErrorText,
  WFormField,
  WIcon,
  WLabel,
  WSelect,
  WShell,
  WTextarea,
  WTextField,
  WAlert,
  WAlertTitle,
  WCard,
  WCardBody,
  WCardTitle,
  WBadge,
  WEmptyState,
  WEmptyStateIcon,
  WEmptyStateTitle,
  WEmptyStateText,
} from '@waypoint/components';
import { finalize, forkJoin } from 'rxjs';

// Traces to: L2-ENGAGE-006, L2-AUTH-010, L2-NFR-001/009/010
@Component({
  selector: 'w-admin-announcements',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    WShell,
    WIcon,
    ReactiveFormsModule,
    DatePipe,
    PercentPipe,
    WButton,
    WFormField,
    WLabel,
    WErrorText,
    WTextField,
    WTextarea,
    WSelect,
    WAlert,
    WAlertTitle,
    WCard,
    WCardBody,
    WCardTitle,
    WBadge,
    WEmptyState,
    WEmptyStateIcon,
    WEmptyStateTitle,
    WEmptyStateText,
  ],
  templateUrl: './admin-announcements.html',
  styles: `
    .announcement-layout {
      display: grid;
      grid-template-columns: minmax(18rem, 0.85fr) minmax(0, 1.35fr);
      gap: var(--w-s5);
      align-items: start;
    }
    .announcement-layout > * {
      min-width: 0;
    }
    .announcement-options {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: var(--w-s3);
      padding: 0;
      border: 0;
    }
    .announcement-option {
      display: flex;
      align-items: center;
      gap: var(--w-s2);
      min-height: 2.75rem;
      padding: var(--w-s2) var(--w-s3);
      border: 1px solid var(--w-border);
      border-radius: var(--w-r-card);
      cursor: pointer;
    }
    .announcement-option:has(input:checked) {
      border-color: var(--w-ink);
      background: var(--w-g100);
    }
    .announcement-option input {
      width: 1.25rem;
      height: 1.25rem;
    }
    .announcement-list {
      margin: 0;
      padding: 0;
      list-style: none;
    }
    .announcement-item {
      min-width: 0;
      overflow-wrap: anywhere;
    }
    .announcement-meta {
      display: flex;
      flex-wrap: wrap;
      gap: var(--w-s2);
      align-items: center;
    }
    .announcement-stats {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: var(--w-s3);
      padding-top: var(--w-s3);
      border-top: 1px solid var(--w-border);
    }
    @media (max-width: 58rem) {
      .announcement-layout {
        grid-template-columns: 1fr;
      }
    }
    @media (max-width: 36rem) {
      .announcement-options,
      .announcement-stats {
        grid-template-columns: 1fr;
      }
      .announcement-submit {
        width: 100%;
        min-height: 45px;
      }
    }
  `,
})
export class AdminAnnouncementsPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(AnnouncementsService);
  private readonly cohortsApi = inject(CohortsService);
  private readonly fb = inject(FormBuilder);

  protected readonly announcements = signal<ManagedAnnouncement[] | undefined>(undefined);
  protected readonly cohorts = signal<CohortSummary[]>([]);
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly outcome = signal<string | null>(null);
  protected readonly targetError = signal<string | null>(null);
  protected readonly scheduleError = signal<string | null>(null);
  protected readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    message: ['', [Validators.required, Validators.maxLength(4000)]],
    audienceScope: ['org' as 'org' | 'cohort', Validators.required],
    cohortId: [''],
    roleTargeting: ['Everyone' as AnnouncementRoleTarget, Validators.required],
    delivery: ['now' as 'now' | 'scheduled', Validators.required],
    scheduledAt: [''],
  });
  private readonly audienceScope = toSignal(this.form.controls.audienceScope.valueChanges, {
    initialValue: this.form.controls.audienceScope.value,
  });
  private readonly delivery = toSignal(this.form.controls.delivery.valueChanges, {
    initialValue: this.form.controls.delivery.value,
  });
  protected readonly isCohort = computed(() => this.audienceScope() === 'cohort');
  protected readonly schedulesLater = computed(() => this.delivery() === 'scheduled');

  constructor() {
    this.load();
  }

  protected compose(): void {
    const value = this.form.getRawValue();
    const missingCohort = value.audienceScope === 'cohort' && !value.cohortId;
    const scheduledDate = value.scheduledAt ? new Date(value.scheduledAt) : null;
    const invalidSchedule =
      value.delivery === 'scheduled' &&
      (!scheduledDate || Number.isNaN(scheduledDate.getTime()) || scheduledDate <= new Date());
    this.targetError.set(missingCohort ? 'Choose a cohort for this audience.' : null);
    this.scheduleError.set(invalidSchedule ? 'Choose a send date and time in the future.' : null);
    if (this.form.invalid || missingCohort || invalidSchedule || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.errorMessage.set(null);
    this.outcome.set(null);
    this.api
      .compose({
        title: value.title.trim(),
        message: value.message.trim(),
        audienceScope: value.audienceScope,
        cohortId: value.audienceScope === 'cohort' ? value.cohortId : null,
        roleTargeting: value.roleTargeting,
        scheduledAt:
          value.delivery === 'scheduled' && scheduledDate ? scheduledDate.toISOString() : null,
      })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (result) => {
          const title = value.title.trim();
          this.outcome.set(result.sentAt ? `${title} was sent.` : `${title} was scheduled.`);
          this.resetForm();
          this.load(false);
        },
        error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
      });
  }

  protected retry(): void {
    this.load();
  }

  protected roleLabel(target: AnnouncementRoleTarget): string {
    return target === 'YouthGuardians' ? 'Youth + guardians' : target;
  }

  private resetForm(): void {
    this.form.reset({
      title: '',
      message: '',
      audienceScope: 'org',
      cohortId: '',
      roleTargeting: 'Everyone',
      delivery: 'now',
      scheduledAt: '',
    });
    this.targetError.set(null);
    this.scheduleError.set(null);
  }

  private load(showLoading = true): void {
    if (showLoading) this.announcements.set(undefined);
    this.errorMessage.set(null);
    forkJoin({
      announcements: this.api.manage(),
      cohorts: this.cohortsApi.list(),
    }).subscribe({
      next: ({ announcements, cohorts }) => {
        this.announcements.set(announcements);
        this.cohorts.set(cohorts);
      },
      error: (error: unknown) => {
        this.announcements.set([]);
        this.errorMessage.set(ApiError.from(error).title);
      },
    });
  }
}
