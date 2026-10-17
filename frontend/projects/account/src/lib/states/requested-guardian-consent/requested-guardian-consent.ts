import { WButton, WFormField, WHelp, WIcon, WLabel } from '@waypoint/components';
import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import {
  ApiError,
  ConsentService,
  ConsentReceipt,
  RequestedConsent,
  RequestedConsentCapture,
} from '@waypoint/api';

// Traces to: L2-CONSENT-001, L2-CONSENT-002, L2-PLATFORM-011
@Component({
  selector: 'w-requested-guardian-consent',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, DatePipe, WIcon, WButton, WFormField, WLabel, WHelp],
  templateUrl: './requested-guardian-consent.html',
})
export class RequestedGuardianConsentPage {
  private readonly api = inject(ConsentService);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly token = this.route.snapshot.queryParamMap.get('token') ?? '';

  protected readonly request = signal<RequestedConsent | undefined>(undefined);
  protected readonly receipt = signal<ConsentReceipt | null>(null);
  protected readonly loadingError = signal<string | null>(null);
  protected readonly submitError = signal<string | null>(null);
  protected readonly submitting = signal(false);
  protected readonly form = this.fb.nonNullable.group({
    guardianName: ['', [Validators.required, Validators.maxLength(128)]],
    relationship: ['', [Validators.required, Validators.maxLength(64)]],
    participationConsent: [false, Validators.requiredTrue],
    mediaConsent: false,
    mediaAllPhotosVideo: false,
    mediaGroupOnly: false,
    mediaFirstNameCaptions: false,
    signature: ['', [Validators.required, Validators.maxLength(128)]],
  });

  constructor() {
    if (!this.token) {
      this.loadingError.set('This consent link is incomplete. Ask the youth to send a new link.');
      return;
    }

    this.api.requested(this.token).subscribe({
      next: (request) => this.request.set(request),
      error: (error: unknown) => this.loadingError.set(ApiError.from(error).title),
    });
  }

  protected submit(): void {
    this.submitError.set(null);
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.api
      .captureRequested(this.token, this.form.getRawValue() as RequestedConsentCapture)
      .subscribe({
        next: (receipt) => {
          this.submitting.set(false);
          this.receipt.set(receipt);
        },
        error: (error: unknown) => {
          this.submitting.set(false);
          this.submitError.set(ApiError.from(error).title);
        },
      });
  }
}
