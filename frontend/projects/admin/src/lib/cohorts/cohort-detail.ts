import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiError, CohortDetail, CohortOptions, CohortsService } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  Button,
  ErrorText,
  FormField,
  Icon,
  IconButton,
  Label,
  Link,
  Select,
  Shell,
  TextField,
  Alert,
  AlertTitle,
  Card,
  CardBody,
  Badge,
  Dialog,
  DialogHeader,
  DialogTitle,
  DialogSubtitle,
  DialogBody,
  DialogFooter,
} from '@waypoint/components';
import { finalize } from 'rxjs';

// Traces to: L2-ADMIN-003
@Component({
  selector: 'w-cohort-detail',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    Shell,
    Icon,
    RouterLink,
    DatePipe,
    ReactiveFormsModule,
    Button,
    IconButton,
    Link,
    FormField,
    Label,
    ErrorText,
    TextField,
    Select,
    Alert,
    AlertTitle,
    Card,
    CardBody,
    Badge,
    Dialog,
    DialogHeader,
    DialogTitle,
    DialogSubtitle,
    DialogBody,
    DialogFooter,
  ],
  templateUrl: './cohort-detail.html',
  styleUrl: './cohort-detail.css',
})
export class CohortDetailPage {
  protected readonly auth = inject(AuthStore);
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(CohortsService);
  private readonly fb = inject(FormBuilder);
  private readonly cohortId = this.route.snapshot.paramMap.get('id') ?? '';

  protected readonly detail = signal<CohortDetail | undefined>(undefined);
  protected readonly options = signal<CohortOptions | undefined>(undefined);
  protected readonly selectedYouthId = signal('');
  protected readonly editOpen = signal(false);
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly outcome = signal<string | null>(null);
  protected readonly editForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(128)]],
    leadMentorId: ['', Validators.required],
    location: ['', Validators.maxLength(256)],
  });

  constructor() {
    this.load();
    this.loadOptions();
  }

  protected selectYouth(id: string): void {
    this.selectedYouthId.set(id);
  }

  protected addYouth(): void {
    const youthId = this.selectedYouthId();
    if (!youthId || this.saving()) return;
    this.saving.set(true);
    this.errorMessage.set(null);
    this.api
      .addYouth(this.cohortId, youthId)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.selectedYouthId.set('');
          this.outcome.set('Youth added to the cohort and enrolled in its track.');
          this.load(false);
        },
        error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
      });
  }

  protected openEdit(): void {
    const cohort = this.detail();
    if (!cohort) return;
    this.editForm.reset({
      name: cohort.name,
      leadMentorId: cohort.leadMentorId ?? '',
      location: cohort.location ?? '',
    });
    this.editOpen.set(true);
  }

  protected closeEdit(): void {
    if (!this.saving()) this.editOpen.set(false);
  }

  protected saveEdit(): void {
    if (this.editForm.invalid || this.saving()) {
      this.editForm.markAllAsTouched();
      return;
    }
    const value = this.editForm.getRawValue();
    this.saving.set(true);
    this.errorMessage.set(null);
    this.api
      .edit(this.cohortId, {
        name: value.name.trim(),
        leadMentorId: value.leadMentorId,
        location: value.location.trim() || null,
      })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.editOpen.set(false);
          this.outcome.set('Cohort details saved.');
          this.load(false);
        },
        error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
      });
  }

  protected retry(): void {
    this.load();
    this.loadOptions();
  }

  protected memberLabel(count: number): string {
    return `${count} ${count === 1 ? 'member' : 'members'}`;
  }

  protected hasDate(value: string): boolean {
    return Boolean(value) && !value.startsWith('0001-');
  }

  private load(showLoading = true): void {
    if (!this.cohortId) {
      this.errorMessage.set('Cohort not found.');
      return;
    }
    if (showLoading) this.detail.set(undefined);
    this.errorMessage.set(null);
    this.api.detail(this.cohortId).subscribe({
      next: (detail) => this.detail.set(detail),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  private loadOptions(): void {
    this.api.options().subscribe({
      next: (options) => this.options.set(options),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }
}
