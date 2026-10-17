import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiError, PublicSiteService } from '@waypoint/api';
import {
  WButton,
  WErrorText,
  WFormField,
  WIcon,
  WLabel,
  WLabelRequired,
  WTextarea,
  WTextField,
  WCheckbox,
  WCheckboxBox,
} from '@waypoint/components';

function isEmail(value: string): boolean {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
}

// Traces to: L2-PUB-003
@Component({
  selector: 'w-get-involved',
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
    WCheckbox,
    WCheckboxBox,
  ],
  templateUrl: './get-involved.html',
})
export class GetInvolved {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(PublicSiteService);

  protected readonly form = this.fb.nonNullable.group({
    name: [''],
    email: [''],
    phone: [''],
    areasOfInterest: [''],
    screeningConsentAck: [false],
  });

  private readonly rawValue = toSignal(this.form.valueChanges, {
    initialValue: this.form.getRawValue(),
  });
  private readonly value = computed(() => {
    const v = this.rawValue();
    return {
      name: v.name ?? '',
      email: v.email ?? '',
      phone: v.phone ?? '',
      areasOfInterest: v.areasOfInterest ?? '',
      screeningConsentAck: v.screeningConsentAck ?? false,
    };
  });

  protected readonly showErrors = signal(false);
  protected readonly submitting = signal(false);
  protected readonly serverError = signal<string | null>(null);
  protected readonly done = signal(false);

  protected readonly errors = computed<Record<string, string>>(() => {
    const v = this.value();
    const e: Record<string, string> = {};
    if (!v.name.trim()) e['name'] = 'Enter your name.';
    if (!isEmail(v.email)) e['email'] = 'Enter a valid email address.';
    // A volunteer working with youth must complete a screening/reference check (AC2).
    if (!v.screeningConsentAck)
      e['screeningConsentAck'] = 'Please acknowledge the screening and reference check.';
    return e;
  });

  protected readonly isValid = computed(() => Object.keys(this.errors()).length === 0);

  protected error(field: string): string {
    return this.showErrors() ? (this.errors()[field] ?? '') : '';
  }

  protected submit(): void {
    this.serverError.set(null);
    this.showErrors.set(true);
    if (!this.isValid()) return;
    this.submitting.set(true);
    const v = this.form.getRawValue();
    this.api
      .submitVolunteerSignup({
        name: v.name.trim(),
        email: v.email.trim(),
        phone: v.phone.trim(),
        areasOfInterest: v.areasOfInterest.trim(),
        screeningConsentAck: v.screeningConsentAck,
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
