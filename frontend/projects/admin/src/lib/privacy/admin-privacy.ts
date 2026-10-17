import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import type { AbstractControl } from '@angular/forms';
import { ApiError, PrivacyService } from '@waypoint/api';
import type { AdminDataSubjectRequest } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WButton, WIcon, WShell } from '@waypoint/components';
import {
  buildVerifiedOwnershipPayload,
  verifiedRecordCount as countVerifiedRecordIds,
  verifiedRecordIdsValidator,
  verifiedRecordSelectionValidator,
} from './verified-record-ids';

// Traces to: L2-NFR-003
@Component({
  selector: 'w-admin-privacy',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, ReactiveFormsModule, WIcon, WShell, WButton],
  templateUrl: './admin-privacy.html',
  styles: `
    .privacy-layout {
      display: grid;
      gap: var(--w-s5);
      grid-template-columns: minmax(0, 1.35fr) minmax(20rem, 0.65fr);
    }
    .privacy-row {
      border-top: 1px solid var(--w-border);
      padding: var(--w-s4) 0;
    }
    .privacy-row:first-child {
      border-top: 0;
    }
    .privacy-meta {
      display: grid;
      gap: var(--w-s2);
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }
    .privacy-meta span {
      overflow-wrap: anywhere;
    }
    .verified-records {
      border: 0;
      border-top: 1px solid var(--w-border);
      margin: var(--w-s2) 0 0;
      padding: var(--w-s5) 0 0;
    }
    .verified-records legend {
      font-size: 1.05rem;
      font-weight: 700;
      padding: 0;
    }
    .verified-records__grid {
      display: grid;
      gap: var(--w-s3);
      grid-template-columns: minmax(0, 1fr);
    }
    .verified-records__grid .w-field {
      margin-bottom: 0;
    }
    .verified-attestation {
      background: var(--w-warning-bg);
      border: 2px solid var(--w-warning);
      border-radius: var(--w-r-card);
      padding: var(--w-s4);
    }
    .verified-attestation .w-check {
      align-items: flex-start;
      margin-bottom: 0;
    }
    .verified-error {
      color: var(--w-error);
      font-size: 0.85rem;
      font-weight: 600;
      margin-top: var(--w-s2);
    }
    @media (max-width: 62rem) {
      .privacy-layout {
        grid-template-columns: 1fr;
      }
    }
    @media (max-width: 36rem) {
      .privacy-meta {
        grid-template-columns: 1fr;
      }
    }
  `,
})
export class AdminPrivacyPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(PrivacyService);
  private readonly fb = inject(FormBuilder);

  protected readonly items = signal<AdminDataSubjectRequest[]>([]);
  protected readonly selected = signal<AdminDataSubjectRequest | null>(null);
  protected readonly status = signal('Pending');
  protected readonly page = signal(1);
  protected readonly total = signal(0);
  protected readonly loading = signal(true);
  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly statusMessage = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group(
    {
      displayName: [''],
      email: [''],
      pronouns: [''],
      clearPronouns: [false],
      resolutionNote: [''],
      verifiedContactRequestIds: ['', verifiedRecordIdsValidator],
      verifiedVolunteerSignupIds: ['', verifiedRecordIdsValidator],
      verifiedDonationPledgeIds: ['', verifiedRecordIdsValidator],
      verifiedApplicantApplicationIds: ['', verifiedRecordIdsValidator],
      verifiedGuardianApplicationIds: ['', verifiedRecordIdsValidator],
      verifiedOwnershipAttested: [false],
    },
    { validators: verifiedRecordSelectionValidator },
  );

  constructor() {
    this.load();
  }

  protected filterChanged(event: Event): void {
    this.status.set((event.target as HTMLSelectElement).value);
    this.page.set(1);
    this.selected.set(null);
    this.load();
  }

  protected review(item: AdminDataSubjectRequest): void {
    this.selected.set(item);
    this.statusMessage.set(null);
    this.form.reset({
      displayName: item.correction?.displayName ?? '',
      email: item.correction?.email ?? '',
      pronouns: item.correction?.pronouns ?? '',
      clearPronouns: item.correction?.clearPronouns ?? false,
      resolutionNote: '',
      verifiedContactRequestIds: '',
      verifiedVolunteerSignupIds: '',
      verifiedDonationPledgeIds: '',
      verifiedApplicantApplicationIds: '',
      verifiedGuardianApplicationIds: '',
      verifiedOwnershipAttested: false,
    });
  }

  protected fulfill(): void {
    const item = this.selected();
    if (!item || this.submitting()) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    this.submitting.set(true);
    this.errorMessage.set(null);
    this.api
      .fulfill(item.id, {
        displayName: value.displayName.trim() || null,
        email: value.email.trim() || null,
        pronouns: value.clearPronouns ? null : value.pronouns.trim() || null,
        clearPronouns: value.clearPronouns,
        resolutionNote: value.resolutionNote.trim() || null,
        ...buildVerifiedOwnershipPayload(value),
      })
      .subscribe({
        next: (result) => {
          this.statusMessage.set(`Request ${result.reference} marked complete.`);
          this.selected.set(null);
          this.submitting.set(false);
          this.load();
        },
        error: (error: unknown) => {
          this.errorMessage.set(ApiError.from(error).title);
          this.submitting.set(false);
        },
      });
  }

  protected clearPronounsChanged(): void {
    if (this.form.controls.clearPronouns.value) {
      this.form.controls.pronouns.reset('');
    }
  }

  protected verifiedRecordCount(): number {
    return countVerifiedRecordIds(this.form.getRawValue());
  }

  protected referenceInvalid(control: AbstractControl<string>): boolean {
    return control.touched && control.hasError('invalidGuidReferences');
  }

  protected invalidReferenceMessage(control: AbstractControl<string>): string {
    const invalid = (control.getError('invalidGuidReferences') as string[] | null) ?? [];
    const preview = invalid.slice(0, 3).join(', ');
    const remainder = invalid.length > 3 ? ` and ${invalid.length - 3} more` : '';
    return `Enter full, non-zero GUIDs only. Invalid: ${preview}${remainder}.`;
  }

  protected attestationInvalid(): boolean {
    return this.form.touched && this.form.hasError('verifiedOwnershipRequired');
  }

  protected applicationOwnershipConflict(): boolean {
    return (
      (this.form.controls.verifiedApplicantApplicationIds.touched ||
        this.form.controls.verifiedGuardianApplicationIds.touched) &&
      this.form.hasError('applicationOwnershipConflict')
    );
  }

  protected previous(): void {
    if (this.page() <= 1) return;
    this.page.update((value) => value - 1);
    this.load();
  }

  protected next(): void {
    if (this.page() * 25 >= this.total()) return;
    this.page.update((value) => value + 1);
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.api.adminRequests(this.status(), this.page(), 25).subscribe({
      next: (result) => {
        this.items.set(result.items);
        this.total.set(result.total);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.errorMessage.set(ApiError.from(error).title);
        this.loading.set(false);
      },
    });
  }
}
