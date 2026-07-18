import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  ApiError,
  OrganizationSettings,
  OrganizationSettingsApi,
  ProgramTerm,
  ProgramTrackSetting,
  SaveProgramTerm,
} from '@wu/api';
import { AuthStore } from '@wu/auth';
import { WuIcon, WuShell } from '@wu/ui';
import { finalize, forkJoin, map } from 'rxjs';

// Traces to: L2-ADMIN-006, L2-CONSENT-001, L2-NFR-009, L2-NFR-010
@Component({
  selector: 'wu-organization-settings',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, WuIcon, ReactiveFormsModule, DatePipe],
  templateUrl: './organization-settings.html',
  styles: `
    .settings-grid {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: var(--wu-sp-5);
      align-items: start;
    }
    .settings-card {
      min-width: 0;
    }
    .logo-preview {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 6rem;
      padding: var(--wu-sp-3);
      border: 1px dashed var(--wu-border);
      border-radius: var(--wu-radius-md);
      background: var(--wu-surface-subtle);
    }
    .logo-preview img {
      max-width: 12rem;
      max-height: 5rem;
      object-fit: contain;
    }
    .term-form {
      display: grid;
      grid-template-columns: minmax(12rem, 2fr) repeat(3, minmax(9rem, 1fr)) auto;
      gap: var(--wu-sp-3);
      align-items: end;
    }
    .term-list,
    .track-list {
      margin: 0;
      padding: 0;
      list-style: none;
    }
    .term-row,
    .track-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: var(--wu-sp-4);
      padding: var(--wu-sp-4) 0;
      border-bottom: 1px solid var(--wu-border);
      overflow-wrap: anywhere;
    }
    .term-row:last-child,
    .track-row:last-child {
      border-bottom: 0;
    }
    .term-row__details,
    .track-row__details {
      min-width: 0;
    }
    .policy-check {
      display: flex;
      align-items: flex-start;
      gap: var(--wu-sp-3);
      min-height: 2.75rem;
    }
    .policy-check input {
      width: 1.25rem;
      height: 1.25rem;
      flex: 0 0 auto;
      margin-top: 0.125rem;
    }
    @media (max-width: 64rem) {
      .term-form {
        grid-template-columns: repeat(2, minmax(0, 1fr));
      }
    }
    @media (max-width: 48rem) {
      .settings-grid,
      .term-form {
        grid-template-columns: 1fr;
      }
      .term-row,
      .track-row {
        flex-direction: column;
        align-items: flex-start;
      }
      .settings-card .wu-btn,
      .term-form .wu-btn,
      .term-row .wu-btn,
      .track-row .wu-btn {
        width: 100%;
        min-height: 45px;
      }
    }
  `,
})
export class OrganizationSettingsPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(OrganizationSettingsApi);
  private readonly fb = inject(FormBuilder);

  protected readonly loaded = signal(false);
  protected readonly terms = signal<ProgramTerm[]>([]);
  protected readonly tracks = signal<ProgramTrackSetting[]>([]);
  protected readonly savingSettings = signal(false);
  protected readonly savingTerm = signal(false);
  protected readonly savingTrackId = signal<string | null>(null);
  protected readonly editingTermId = signal<string | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly outcome = signal<string | null>(null);
  protected readonly settingsForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(128)]],
    displayName: ['', Validators.maxLength(128)],
    logoUrl: ['', Validators.maxLength(2048)],
    city: ['', Validators.maxLength(128)],
    contactEmail: ['', Validators.email],
    consentStatement: ['', Validators.maxLength(4000)],
    requireReConsentEachTerm: true,
    requireMediaConsent: false,
  });
  protected readonly termForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(128)]],
    startDate: ['', Validators.required],
    endDate: ['', Validators.required],
    status: ['Upcoming' as ProgramTerm['status'], Validators.required],
  });

  constructor() {
    this.load();
  }

  protected saveSettings(): void {
    if (this.settingsForm.invalid || this.savingSettings()) {
      this.settingsForm.markAllAsTouched();
      return;
    }
    const value = this.settingsForm.getRawValue();
    const settings: OrganizationSettings = {
      name: value.name.trim(),
      displayName: this.clean(value.displayName),
      logoUrl: this.clean(value.logoUrl),
      city: this.clean(value.city),
      contactEmail: this.clean(value.contactEmail),
      consentStatement: this.clean(value.consentStatement),
      requireReConsentEachTerm: value.requireReConsentEachTerm,
      requireMediaConsent: value.requireMediaConsent,
    };
    this.savingSettings.set(true);
    this.errorMessage.set(null);
    this.outcome.set(null);
    this.api
      .update(settings)
      .pipe(finalize(() => this.savingSettings.set(false)))
      .subscribe({
        next: () => this.outcome.set('Organization settings saved and recorded in the audit log.'),
        error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
      });
  }

  protected saveTerm(): void {
    if (this.termForm.invalid || this.savingTerm()) {
      this.termForm.markAllAsTouched();
      return;
    }
    const value = this.termForm.getRawValue();
    if (value.endDate <= value.startDate) {
      this.errorMessage.set('The term end date must be after its start date.');
      return;
    }
    const term: SaveProgramTerm = {
      name: value.name.trim(),
      startDate: value.startDate,
      endDate: value.endDate,
      status: value.status,
    };
    const editingId = this.editingTermId();
    const request = editingId
      ? this.api.updateTerm(editingId, term)
      : this.api.createTerm(term).pipe(map(() => undefined));
    this.savingTerm.set(true);
    this.errorMessage.set(null);
    this.outcome.set(null);
    request.pipe(finalize(() => this.savingTerm.set(false))).subscribe({
      next: () => {
        this.outcome.set(
          `Term ${editingId ? 'updated' : 'created'} and recorded in the audit log.`,
        );
        this.cancelTermEdit();
        this.loadTerms();
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  protected editTerm(term: ProgramTerm): void {
    this.editingTermId.set(term.id);
    this.termForm.reset({
      name: term.name,
      startDate: term.startDate.slice(0, 10),
      endDate: term.endDate.slice(0, 10),
      status: term.status,
    });
  }

  protected cancelTermEdit(): void {
    this.editingTermId.set(null);
    this.termForm.reset({ name: '', startDate: '', endDate: '', status: 'Upcoming' });
  }

  protected toggleTrack(track: ProgramTrackSetting): void {
    if (this.savingTrackId()) return;
    this.savingTrackId.set(track.id);
    this.errorMessage.set(null);
    this.outcome.set(null);
    this.api
      .setTrackEnabled(track.id, !track.enabled)
      .pipe(finalize(() => this.savingTrackId.set(null)))
      .subscribe({
        next: () => {
          this.tracks.update((tracks) =>
            tracks.map((item) =>
              item.id === track.id ? { ...item, enabled: !item.enabled } : item,
            ),
          );
          this.outcome.set(
            `${track.name} ${track.enabled ? 'paused' : 'enabled'} and recorded in the audit log.`,
          );
        },
        error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
      });
  }

  protected retry(): void {
    this.load();
  }

  private load(): void {
    this.loaded.set(false);
    this.errorMessage.set(null);
    forkJoin({
      settings: this.api.get(),
      terms: this.api.terms(),
      tracks: this.api.tracks(),
    }).subscribe({
      next: ({ settings, terms, tracks }) => {
        this.settingsForm.reset({
          name: settings.name,
          displayName: settings.displayName ?? '',
          logoUrl: settings.logoUrl ?? '',
          city: settings.city ?? '',
          contactEmail: settings.contactEmail ?? '',
          consentStatement: settings.consentStatement ?? '',
          requireReConsentEachTerm: settings.requireReConsentEachTerm,
          requireMediaConsent: settings.requireMediaConsent,
        });
        this.terms.set(terms);
        this.tracks.set(tracks);
        this.loaded.set(true);
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  private loadTerms(): void {
    this.api.terms().subscribe({
      next: (terms) => this.terms.set(terms),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  private clean(value: string): string | null {
    return value.trim() || null;
  }
}
