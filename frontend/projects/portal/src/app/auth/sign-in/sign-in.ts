import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiError } from '@wu/api';
import { AuthStore, roleHome } from '@wu/auth';
import { WuIcon } from '@wu/ui';

@Component({
  selector: 'wu-sign-in',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, WuIcon],
  templateUrl: './sign-in.html',
})
export class SignIn {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
    remember: [false],
  });

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly showPassword = signal(false);

  protected togglePassword(): void {
    this.showPassword.update((v) => !v);
  }

  protected submit(): void {
    this.errorMessage.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { email, password, remember } = this.form.getRawValue();
    this.submitting.set(true);
    this.auth.signIn(email, password, remember).subscribe({
      next: (result) => {
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        void this.router.navigateByUrl(returnUrl || roleHome(result.role));
      },
      error: (err: unknown) => {
        this.submitting.set(false);
        this.handleError(ApiError.from(err));
      },
    });
  }

  private handleError(err: ApiError): void {
    switch (err.status) {
      case 423:
        void this.router.navigate(['/auth/account-locked'], { state: { message: err.title } });
        return;
      case 403:
        void this.router.navigateByUrl('/auth/account-deactivated');
        return;
      default:
        // 400 (invalid credentials — identical copy for unknown email vs wrong
        // password), 0/500, etc. Shown inline; never reveals whether the email exists.
        this.errorMessage.set(err.title);
    }
  }
}
