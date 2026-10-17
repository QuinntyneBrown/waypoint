import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { APPLICANT_MAX_AGE, APPLICANT_MIN_AGE, ApiError, IntakeService } from '@waypoint/api';
import {
  WButton,
  WErrorText,
  WFormField,
  WIcon,
  WLabel,
  WLabelRequired,
  WSelect,
  WTextarea,
  WTextField,
} from '@waypoint/components';

function isEmail(value: string): boolean {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
}

// Traces to: L2-INTAKE-001
@Component({
  selector: 'w-apply',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    WIcon,
    WButton,
    WFormField,
    WLabel,
    WLabelRequired,
    WErrorText,
    WTextField,
    WTextarea,
    WSelect,
  ],
  templateUrl: './apply.html',
})
export class Apply {
  private readonly fb = inject(FormBuilder);
  private readonly intake = inject(IntakeService);

  protected readonly minAge = APPLICANT_MIN_AGE;
  protected readonly maxAge = APPLICANT_MAX_AGE;

  protected readonly form = this.fb.nonNullable.group({
    whoIsFilling: ['self'],
    firstName: [''],
    lastName: [''],
    age: [''],
    neighbourhood: [''],
    cohortInterest: [''],
    trackInterest: [''],
    whyInterested: [''],
    guardianName: [''],
    guardianEmail: [''],
    guardianPhone: [''],
    participationConsentAck: [false],
    privacyAck: [false],
  });

  private readonly rawValue = toSignal(this.form.valueChanges, {
    initialValue: this.form.getRawValue(),
  });
  // valueChanges emits a partial; coalesce every field to its non-undefined shape.
  private readonly value = computed(() => {
    const v = this.rawValue();
    return {
      whoIsFilling: v.whoIsFilling ?? '',
      firstName: v.firstName ?? '',
      lastName: v.lastName ?? '',
      age: v.age ?? '',
      neighbourhood: v.neighbourhood ?? '',
      cohortInterest: v.cohortInterest ?? '',
      trackInterest: v.trackInterest ?? '',
      whyInterested: v.whyInterested ?? '',
      guardianName: v.guardianName ?? '',
      guardianEmail: v.guardianEmail ?? '',
      guardianPhone: v.guardianPhone ?? '',
      participationConsentAck: v.participationConsentAck ?? false,
      privacyAck: v.privacyAck ?? false,
    };
  });

  protected readonly showErrors = signal(false);
  protected readonly submitting = signal(false);
  protected readonly serverError = signal<string | null>(null);
  protected readonly done = signal(false);

  /** Under-18 applicants must supply guardian contact details (AC2). */
  protected readonly needsGuardian = computed(() => {
    const age = Number(this.value().age);
    return this.value().age !== '' && !Number.isNaN(age) && age < 18;
  });

  protected readonly errors = computed<Record<string, string>>(() => {
    const v = this.value();
    const e: Record<string, string> = {};
    if (!['self', 'guardian', 'referrer'].includes(v.whoIsFilling))
      e['whoIsFilling'] = 'Choose who is filling out the application.';
    if (!v.firstName.trim()) e['firstName'] = 'Enter a first name.';
    if (!v.lastName.trim()) e['lastName'] = 'Enter a last name.';
    const age = Number(v.age);
    if (v.age === '') e['age'] = 'Enter an age.';
    else if (Number.isNaN(age) || age < APPLICANT_MIN_AGE || age > APPLICANT_MAX_AGE)
      e['age'] = `Applicants must be aged ${APPLICANT_MIN_AGE}–${APPLICANT_MAX_AGE} inclusive.`;
    if (!v.neighbourhood.trim()) e['neighbourhood'] = 'Enter a neighbourhood.';
    if (!v.cohortInterest.trim()) e['cohortInterest'] = 'Choose a cohort of interest.';
    if (!v.whyInterested.trim()) e['whyInterested'] = 'Tell us why you’re interested.';
    if (this.needsGuardian()) {
      if (!v.guardianName.trim())
        e['guardianName'] = 'A guardian name is required for under-18 applicants.';
      if (!isEmail(v.guardianEmail)) e['guardianEmail'] = 'A valid guardian email is required.';
      if (!v.guardianPhone.trim()) e['guardianPhone'] = 'A guardian phone number is required.';
    }
    if (!v.participationConsentAck)
      e['participationConsentAck'] = 'Please acknowledge participation consent.';
    if (!v.privacyAck) e['privacyAck'] = 'Please acknowledge the privacy notice.';
    return e;
  });

  protected readonly isValid = computed(() => Object.keys(this.errors()).length === 0);
  /** The age gate shows live once an age is entered (independent of a submit attempt). */
  protected readonly ageGateError = computed(() =>
    this.value().age === '' ? '' : (this.errors()['age'] ?? ''),
  );

  protected error(field: string): string {
    return this.showErrors() ? (this.errors()[field] ?? '') : '';
  }

  protected submit(): void {
    this.serverError.set(null);
    this.showErrors.set(true);
    if (!this.isValid()) return;
    this.submitting.set(true);
    const v = this.form.getRawValue();
    this.intake
      .submit({
        whoIsFilling: v.whoIsFilling as 'self' | 'guardian' | 'referrer',
        firstName: v.firstName.trim(),
        lastName: v.lastName.trim(),
        age: Number(v.age),
        neighbourhood: v.neighbourhood.trim(),
        cohortInterest: v.cohortInterest.trim(),
        whyInterested: v.whyInterested.trim(),
        trackInterest: v.trackInterest || null,
        guardianName: this.needsGuardian() ? v.guardianName.trim() : null,
        guardianEmail: this.needsGuardian() ? v.guardianEmail.trim() : null,
        guardianPhone: this.needsGuardian() ? v.guardianPhone.trim() : null,
        participationConsentAck: v.participationConsentAck,
        privacyAck: v.privacyAck,
      })
      .subscribe({
        next: () => this.done.set(true),
        error: (err: unknown) => {
          this.submitting.set(false);
          this.serverError.set(ApiError.from(err).title);
        },
      });
  }
}
