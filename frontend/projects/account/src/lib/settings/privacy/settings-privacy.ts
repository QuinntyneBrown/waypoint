import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  ApiError,
  DataSubjectAction,
  DataSubjectRequestResult,
  MyData,
  PrivacyService,
} from '@waypoint/api';
import { forkJoin } from 'rxjs';
import { SettingsShell } from '../settings-shell';
import {
  WButton,
  WErrorText,
  WFormField,
  WLabel,
  WSelect,
  WTextarea,
  WTextField,
} from '@waypoint/components';

// Traces to: L2-NFR-003
@Component({
  selector: 'w-settings-privacy',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    ReactiveFormsModule,
    SettingsShell,
    WButton,
    WFormField,
    WLabel,
    WErrorText,
    WTextField,
    WTextarea,
    WSelect,
  ],
  templateUrl: './settings-privacy.html',
  styles: `
    .privacy-grid {
      display: grid;
      gap: var(--w-s5);
      grid-template-columns: minmax(0, 1fr) minmax(0, 1fr);
    }
    .privacy-data {
      display: grid;
      gap: var(--w-s3);
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }
    .privacy-data dt {
      color: var(--w-muted);
      font-size: 0.85rem;
    }
    .privacy-data dd {
      margin: 0.2rem 0 0;
      overflow-wrap: anywhere;
    }
    .request-row {
      border-top: 1px solid var(--w-border);
      padding: var(--w-s4) 0;
    }
    @media (max-width: 48rem) {
      .privacy-grid,
      .privacy-data {
        grid-template-columns: 1fr;
      }
    }
  `,
})
export class SettingsPrivacyPage {
  private static readonly requestPageSize = 25;
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(PrivacyService);

  protected readonly data = signal<MyData | null>(null);
  protected readonly requests = signal<DataSubjectRequestResult[]>([]);
  protected readonly requestPage = signal(1);
  protected readonly requestTotal = signal(0);
  protected readonly canExport = signal(false);
  protected readonly loading = signal(true);
  protected readonly loadingMore = signal(false);
  protected readonly downloading = signal(false);
  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly acknowledgement = signal<DataSubjectRequestResult | null>(null);
  protected readonly hasMoreRequests = computed(() => this.requests().length < this.requestTotal());

  protected readonly form = this.fb.nonNullable.group({
    actionType: ['access' as DataSubjectAction, Validators.required],
    details: ['', Validators.maxLength(4000)],
    displayName: [''],
    email: ['', Validators.email],
    pronouns: [''],
    clearPronouns: [false],
  });

  constructor() {
    this.load();
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    this.submitting.set(true);
    this.errorMessage.set(null);
    this.api
      .request({
        actionType: value.actionType,
        details: value.details.trim() || null,
        correction:
          value.actionType === 'correction'
            ? {
                displayName: value.displayName.trim() || null,
                email: value.email.trim() || null,
                pronouns: value.clearPronouns ? null : value.pronouns.trim() || null,
                clearPronouns: value.clearPronouns,
              }
            : null,
      })
      .subscribe({
        next: (result) => {
          this.acknowledgement.set(result);
          this.form.controls.details.reset('');
          this.submitting.set(false);
          this.reloadRequests();
        },
        error: (error: unknown) => {
          this.errorMessage.set(ApiError.from(error).title);
          this.submitting.set(false);
        },
      });
  }

  protected downloadPersonalData(): void {
    if (!this.canExport() || this.downloading()) return;
    this.downloading.set(true);
    this.errorMessage.set(null);
    this.api.exportMyData().subscribe({
      next: (response) => {
        const blob = response.body;
        if (!blob) {
          this.errorMessage.set('The data export was empty. Please try again.');
          this.downloading.set(false);
          return;
        }
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = this.exportFileName(response.headers.get('content-disposition'));
        anchor.click();
        URL.revokeObjectURL(url);
        this.downloading.set(false);
      },
      error: (error: unknown) => {
        this.errorMessage.set(ApiError.from(error).title);
        this.downloading.set(false);
      },
    });
  }

  protected loadMoreRequests(): void {
    if (this.loadingMore() || !this.hasMoreRequests()) return;
    this.loadingMore.set(true);
    this.errorMessage.set(null);
    const nextPage = this.requestPage() + 1;
    this.api.myRequests(nextPage, SettingsPrivacyPage.requestPageSize).subscribe({
      next: (result) => {
        const known = new Set(this.requests().map((item) => item.id));
        this.requests.update((items) => [
          ...items,
          ...result.items.filter((item) => !known.has(item.id)),
        ]);
        this.requestPage.set(result.page);
        this.requestTotal.set(result.total);
        this.canExport.set(result.canExport);
        this.loadingMore.set(false);
      },
      error: (error: unknown) => {
        this.errorMessage.set(ApiError.from(error).title);
        this.loadingMore.set(false);
      },
    });
  }

  protected clearPronounsChanged(): void {
    if (this.form.controls.clearPronouns.value) {
      this.form.controls.pronouns.reset('');
    }
  }

  private load(): void {
    forkJoin({
      data: this.api.myData(),
      requests: this.api.myRequests(1, SettingsPrivacyPage.requestPageSize),
    }).subscribe({
      next: ({ data, requests }) => {
        this.data.set(data);
        this.requests.set(requests.items);
        this.requestPage.set(requests.page);
        this.requestTotal.set(requests.total);
        this.canExport.set(requests.canExport);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.errorMessage.set(ApiError.from(error).title);
        this.loading.set(false);
      },
    });
  }

  private reloadRequests(): void {
    this.api.myRequests(1, SettingsPrivacyPage.requestPageSize).subscribe({
      next: (result) => {
        this.requests.set(result.items);
        this.requestPage.set(result.page);
        this.requestTotal.set(result.total);
        this.canExport.set(result.canExport);
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  private exportFileName(contentDisposition: string | null): string {
    const encoded = contentDisposition?.match(/filename\*=UTF-8''([^;]+)/i)?.[1];
    const quoted = contentDisposition?.match(/filename="?([^";]+)"?/i)?.[1];
    const fromHeader = encoded ? decodeURIComponent(encoded) : quoted;
    const safe = fromHeader?.replace(/[\\/]/g, '-').trim();
    return safe || `waypoint-personal-data-${this.data()?.id ?? 'export'}.json`;
  }
}
