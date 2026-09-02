import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiError, isPasswordCompliant, PASSWORD_POLICY_MESSAGE, PasswordResetApi } from '@w/api';
import { WIcon } from '@w/ui';

// Traces to: L2-AUTH-005
@Component({
  selector: 'w-reset-password',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, WIcon],
  templateUrl: './reset-password.html',
})
export class ResetPassword {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(PasswordResetApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  private readonly token = this.route.snapshot.queryParamMap.get('token') ?? '';

  protected readonly policyMessage = PASSWORD_POLICY_MESSAGE;
  protected readonly form = this.fb.nonNullable.group({
    newPassword: ['', [Validators.required]],
    confirmPassword: ['', [Validators.required]],
  });

  private readonly value = toSignal(this.form.valueChanges, {
    initialValue: this.form.getRawValue(),
  });

  protected readonly showNew = signal(false);
  protected readonly showConfirm = signal(false);
  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly policyOk = computed(() => isPasswordCompliant(this.value().newPassword ?? ''));
  protected readonly confirmTouched = computed(
    () => (this.value().confirmPassword ?? '').length > 0,
  );
  protected readonly matches = computed(
    () => (this.value().newPassword ?? '') === (this.value().confirmPassword ?? ''),
  );
  protected readonly canSubmit = computed(
    () => this.policyOk() && this.matches() && this.confirmTouched(),
  );

  protected submit(): void {
    this.errorMessage.set(null);
    if (!this.canSubmit()) return;
    const { newPassword, confirmPassword } = this.form.getRawValue();
    this.submitting.set(true);
    this.api.confirmReset({ token: this.token, newPassword, confirmPassword }).subscribe({
      next: () => void this.router.navigateByUrl('/auth/reset-success'),
      error: (err: unknown) => {
        this.submitting.set(false);
        const apiError = ApiError.from(err);
        if (apiError.status === 410) {
          void this.router.navigateByUrl('/auth/reset-link-invalid');
          return;
        }
        this.errorMessage.set(apiError.title);
      },
    });
  }
}
