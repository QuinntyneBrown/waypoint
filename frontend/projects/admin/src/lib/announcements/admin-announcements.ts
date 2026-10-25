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
  Button,
  ErrorText,
  FormField,
  Icon,
  Label,
  Select,
  Shell,
  Textarea,
  TextField,
  Alert,
  AlertTitle,
  Card,
  CardBody,
  CardTitle,
  Badge,
  EmptyState,
  EmptyStateIcon,
  EmptyStateTitle,
  EmptyStateText,
  StatLabel,
  StatValue,
} from '@waypoint/components';
import { finalize, forkJoin } from 'rxjs';

// Traces to: L2-ENGAGE-006, L2-AUTH-010, L2-NFR-001/009/010
@Component({
  selector: 'ph-admin-announcements',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    Shell,
    Icon,
    ReactiveFormsModule,
    DatePipe,
    PercentPipe,
    Button,
    FormField,
    Label,
    ErrorText,
    TextField,
    Textarea,
    Select,
    Alert,
    AlertTitle,
    Card,
    CardBody,
    CardTitle,
    Badge,
    EmptyState,
    EmptyStateIcon,
    EmptyStateTitle,
    EmptyStateText,
    StatLabel,
    StatValue,
  ],
  templateUrl: './admin-announcements.html',
  styleUrl: './admin-announcements.css',
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
