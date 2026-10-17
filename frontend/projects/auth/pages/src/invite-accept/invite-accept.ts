import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthStore, roleHome } from '@waypoint/auth';
import { ApiError, InvitationContext, InvitationsService } from '@waypoint/api';
import { WButton, WIcon } from '@waypoint/components';

// Traces to: L2-AUTH-004, L2-AUTH-008, L2-PLATFORM-011
@Component({
  selector: 'w-invite-accept',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, WIcon, WButton],
  templateUrl: './invite-accept.html',
})
export class InviteAcceptPage {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(InvitationsService);
  private readonly auth = inject(AuthStore);

  protected readonly token = this.route.snapshot.queryParamMap.get('token') ?? '';
  protected readonly context = signal<InvitationContext | null>(null);
  protected readonly loading = signal(true);
  protected readonly submitting = signal(false);
  protected readonly requesting = signal(false);
  protected readonly replacementRequested = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly showPassword = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    displayName: ['', [Validators.required, Validators.maxLength(120)]],
    password: [
      '',
      [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d).+$/),
      ],
    ],
    confirmPassword: ['', Validators.required],
    agreeToGuidelines: [false, Validators.requiredTrue],
  });

  constructor() {
    if (!this.token) {
      this.context.set({
        state: 'invalid',
        role: null,
        cohortName: null,
        mentorName: null,
        expiresAt: null,
        maskedEmail: null,
      });
      this.loading.set(false);
      return;
    }
    this.api.context(this.token).subscribe({
      next: (context) => {
        this.context.set(context);
        this.loading.set(false);
      },
      error: () => {
        this.context.set({
          state: 'invalid',
          role: null,
          cohortName: null,
          mentorName: null,
          expiresAt: null,
          maskedEmail: null,
        });
        this.loading.set(false);
      },
    });
  }

  protected togglePassword(): void {
    this.showPassword.update((value) => !value);
  }

  protected accept(): void {
    this.errorMessage.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    if (value.password !== value.confirmPassword) {
      this.errorMessage.set('The passwords do not match.');
      return;
    }
    this.submitting.set(true);
    this.auth
      .acceptInvitation({
        token: this.token,
        displayName: value.displayName.trim(),
        password: value.password,
        agreeToGuidelines: value.agreeToGuidelines,
      })
      .subscribe({
        next: (result) =>
          void this.router.navigateByUrl(
            result.role === 'Youth' ? '/onboarding' : roleHome(result.role),
          ),
        error: (error: unknown) => {
          const apiError = ApiError.from(error);
          if (apiError.status === 404 || apiError.status === 409 || apiError.status === 410) {
            this.context.update((context) =>
              context ? { ...context, state: 'invalid' } : context,
            );
          }
          this.errorMessage.set(apiError.title);
          this.submitting.set(false);
        },
      });
  }

  protected requestReplacement(): void {
    if (!this.token || this.requesting()) return;
    this.requesting.set(true);
    this.errorMessage.set(null);
    this.api.requestReplacement(this.token).subscribe({
      next: () => {
        this.requesting.set(false);
        this.replacementRequested.set(true);
      },
      error: () => {
        // Enumeration-safe: the user sees the same acknowledgement even if no inviter can be found.
        this.requesting.set(false);
        this.replacementRequested.set(true);
      },
    });
  }

  protected stateHeading(state: InvitationContext['state']): string {
    switch (state) {
      case 'expired':
        return 'This invite link has expired';
      case 'revoked':
        return 'This invite is no longer active';
      case 'accepted':
        return 'This invite has already been used';
      default:
        return 'This invite link is not valid';
    }
  }
}
