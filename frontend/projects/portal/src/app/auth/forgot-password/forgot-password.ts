import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { PasswordResetApi } from '@w/api';

// Traces to: L2-AUTH-005
@Component({
  selector: 'w-forgot-password',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './forgot-password.html',
})
export class ForgotPassword {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(PasswordResetApi);
  private readonly router = inject(Router);

  protected readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
  });
  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected submit(): void {
    this.errorMessage.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { email } = this.form.getRawValue();
    this.submitting.set(true);
    this.api.requestReset({ email }).subscribe({
      // The endpoint always succeeds (204) whether or not the account exists.
      next: () => void this.router.navigate(['/auth/reset-link-sent'], { state: { email } }),
      error: () => {
        this.submitting.set(false);
        this.errorMessage.set('We couldn’t send the reset link. Please try again.');
      },
    });
  }
}
