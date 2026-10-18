import { DatePipe, DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiError, GuardianService, GuardianConsentRecord } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  Button,
  FormField,
  Icon,
  Label,
  Shell,
  TextField,
  Checkbox,
  CheckboxBox,
  Alert,
  Card,
  CardBody,
  CardTitle,
  Badge,
  EmptyState,
  EmptyStateTitle,
  EmptyStateText,
} from '@waypoint/components';
import { GuardianContextService } from '../guardian-context.service';

// Traces to: L2-GUARDIAN-003, L2-CONSENT-001
@Component({
  selector: 'w-guardian-consent',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    Shell,
    ReactiveFormsModule,
    DatePipe,
    Icon,
    Button,
    FormField,
    Label,
    TextField,
    Checkbox,
    CheckboxBox,
    Alert,
    Card,
    CardBody,
    CardTitle,
    Badge,
    EmptyState,
    EmptyStateTitle,
    EmptyStateText,
  ],
  templateUrl: './guardian-consent.html',
})
export class GuardianConsent {
  protected readonly auth = inject(AuthStore);
  protected readonly context = inject(GuardianContextService);
  private readonly api = inject(GuardianService);
  private readonly fb = inject(FormBuilder);
  private readonly doc = inject(DOCUMENT);

  protected readonly records = signal<GuardianConsentRecord[] | undefined>(undefined);
  protected readonly statusMessage = signal<string | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly contactForm = this.fb.nonNullable.group({
    displayName: ['', [Validators.required, Validators.maxLength(128)]],
    email: ['', [Validators.required, Validators.email]],
    relationship: ['', Validators.maxLength(64)],
    phone: ['', Validators.maxLength(32)],
    emergencyContactName: ['', Validators.maxLength(128)],
    emergencyContactPhone: ['', Validators.maxLength(32)],
  });
  protected readonly mediaForm = this.fb.nonNullable.group({
    mediaConsent: false,
    mediaAllPhotosVideo: false,
    mediaGroupOnly: false,
    mediaFirstNameCaptions: false,
  });

  constructor() {
    this.context.load();
    this.loadContact();
    effect(() => {
      const childId = this.context.selectedChildId();
      if (childId) this.loadConsent(childId);
      else if (this.context.children() !== undefined) this.records.set([]);
    });
  }

  protected saveContact(): void {
    if (this.contactForm.invalid || this.saving()) {
      this.contactForm.markAllAsTouched();
      return;
    }
    this.saving.set(true);
    this.api.updateContact(this.contactForm.getRawValue()).subscribe({
      next: () => this.finish('Contact details saved.'),
      error: (error: unknown) => this.fail(error),
    });
  }

  protected saveMedia(): void {
    const childId = this.context.selectedChildId();
    if (!childId || this.saving()) return;
    this.saving.set(true);
    this.api.updateMediaConsent(childId, this.mediaForm.getRawValue()).subscribe({
      next: () => {
        const update = this.mediaForm.getRawValue();
        this.records.update((records) =>
          records?.map((record, index) => (index === 0 ? { ...record, ...update } : record)),
        );
        this.finish('Media consent saved.');
      },
      error: (error: unknown) => this.fail(error),
    });
  }

  protected emailCopy(consentId: string): void {
    if (this.saving()) return;
    this.saving.set(true);
    this.api.emailConsentCopy(consentId).subscribe({
      next: () => this.finish('Your consent record has been emailed.'),
      error: (error: unknown) => this.fail(error),
    });
  }

  protected withdraw(consentId: string): void {
    if (
      this.saving() ||
      !this.doc.defaultView?.confirm(
        'Are you sure you want to withdraw participation and media consent? This may restrict your youth’s program access.',
      )
    ) {
      return;
    }
    this.saving.set(true);
    this.api.withdrawConsent(consentId).subscribe({
      next: () => {
        const withdrawnAt = new Date().toISOString();
        this.records.update((records) =>
          records?.map((record) =>
            record.consentId === consentId ? { ...record, withdrawnAt } : record,
          ),
        );
        this.finish('Consent withdrawn. The program team has been notified.');
      },
      error: (error: unknown) => this.fail(error),
    });
  }

  private loadContact(): void {
    this.api.contact().subscribe({
      next: (contact) =>
        this.contactForm.reset({
          displayName: contact.displayName,
          email: contact.email,
          relationship: contact.relationship ?? '',
          phone: contact.phone ?? '',
          emergencyContactName: contact.emergencyContactName ?? '',
          emergencyContactPhone: contact.emergencyContactPhone ?? '',
        }),
      error: (error: unknown) => this.fail(error),
    });
  }

  private loadConsent(childId: string): void {
    this.records.set(undefined);
    this.api.consent(childId).subscribe({
      next: (records) => {
        this.records.set(records);
        const current = records[0];
        if (current) {
          this.mediaForm.reset({
            mediaConsent: current.mediaConsent,
            mediaAllPhotosVideo: current.mediaAllPhotosVideo,
            mediaGroupOnly: current.mediaGroupOnly,
            mediaFirstNameCaptions: current.mediaFirstNameCaptions,
          });
        }
      },
      error: (error: unknown) => {
        this.records.set([]);
        this.fail(error);
      },
    });
  }

  private finish(message: string): void {
    this.saving.set(false);
    this.errorMessage.set(null);
    this.statusMessage.set(message);
  }

  private fail(error: unknown): void {
    this.saving.set(false);
    this.errorMessage.set(ApiError.from(error).title);
  }
}
