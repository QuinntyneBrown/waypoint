import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiError, CohortOptions, CohortsApi, CohortSummary } from '@wu/api';
import { AuthStore } from '@wu/auth';
import { WuAccessibleDialog, WuIcon, WuShell } from '@wu/ui';
import { finalize } from 'rxjs';

// Traces to: L2-ADMIN-003
@Component({
  selector: 'wu-cohorts-list',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, WuIcon, WuAccessibleDialog, RouterLink, DatePipe, ReactiveFormsModule],
  templateUrl: './cohorts-list.html',
  styles: `
    .cohort-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(min(100%, 19rem), 1fr));
      gap: var(--wu-sp-4);
    }
    .cohort-card {
      height: 100%;
    }
    .cohort-card__meta {
      display: grid;
      gap: var(--wu-sp-2);
      overflow-wrap: anywhere;
    }
    .form-grid {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: var(--wu-sp-4);
    }
    .form-grid__wide {
      grid-column: 1 / -1;
    }
    @media (max-width: 42rem) {
      .form-grid {
        grid-template-columns: 1fr;
      }
      .form-grid__wide {
        grid-column: auto;
      }
      .wu-modal__footer .wu-btn {
        flex: 1 1 100%;
        min-height: 45px;
      }
    }
  `,
})
export class CohortsListPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(CohortsApi);
  private readonly fb = inject(FormBuilder);

  protected readonly cohorts = signal<CohortSummary[] | undefined>(undefined);
  protected readonly options = signal<CohortOptions | undefined>(undefined);
  protected readonly createOpen = signal(false);
  protected readonly saving = signal(false);
  protected readonly dateError = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly outcome = signal<string | null>(null);
  protected readonly loading = computed(() => this.cohorts() === undefined);
  protected readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(128)]],
    trackId: ['', Validators.required],
    leadMentorId: ['', Validators.required],
    startDate: [''],
    endDate: [''],
    location: ['', Validators.maxLength(256)],
  });

  constructor() {
    this.load();
    this.loadOptions();
  }

  protected openCreate(): void {
    this.form.reset();
    this.dateError.set(false);
    this.createOpen.set(true);
  }

  protected closeCreate(): void {
    if (!this.saving()) this.createOpen.set(false);
  }

  protected create(): void {
    const value = this.form.getRawValue();
    const datesInvalid = Boolean(
      value.startDate && value.endDate && value.endDate <= value.startDate,
    );
    this.dateError.set(datesInvalid);
    if (this.form.invalid || datesInvalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.errorMessage.set(null);
    this.api
      .create({
        name: value.name.trim(),
        trackId: value.trackId,
        leadMentorId: value.leadMentorId,
        startDate: value.startDate || null,
        endDate: value.endDate || null,
        location: value.location.trim() || null,
      })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.createOpen.set(false);
          this.outcome.set(`${value.name.trim()} was created.`);
          this.load(false);
        },
        error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
      });
  }

  protected retry(): void {
    this.load();
    this.loadOptions();
  }

  protected hasDate(value: string): boolean {
    return Boolean(value) && !value.startsWith('0001-');
  }

  private load(showLoading = true): void {
    if (showLoading) this.cohorts.set(undefined);
    this.errorMessage.set(null);
    this.api.list().subscribe({
      next: (cohorts) => this.cohorts.set(cohorts),
      error: (error: unknown) => {
        this.cohorts.set([]);
        this.errorMessage.set(ApiError.from(error).title);
      },
    });
  }

  private loadOptions(): void {
    this.api.options().subscribe({
      next: (options) => this.options.set(options),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }
}
