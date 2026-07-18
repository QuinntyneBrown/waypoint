import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiError, isPasswordCompliant, PASSWORD_POLICY_MESSAGE, ProfileApi } from '@wu/api';
import { AuthStore } from '@wu/auth';
import { ToastService, WuIcon } from '@wu/ui';
import { SettingsShell } from '../settings-shell';

// Traces to: L2-PLATFORM-002
@Component({
  selector: 'wu-settings-profile',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [SettingsShell, ReactiveFormsModule, WuIcon],
  templateUrl: './settings-profile.html',
})
export class SettingsProfile {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ProfileApi);
  private readonly auth = inject(AuthStore);
  private readonly toasts = inject(ToastService);

  protected readonly policyMessage = PASSWORD_POLICY_MESSAGE;

  protected readonly profile = this.fb.nonNullable.group({
    displayName: [this.auth.user()?.displayName ?? '', [Validators.required]],
    email: [this.auth.user()?.email ?? '', [Validators.required, Validators.email]],
    pronouns: [''],
    photoUrl: [''],
  });
  protected readonly savingProfile = signal(false);

  protected readonly password = this.fb.nonNullable.group({
    currentPassword: [''],
    newPassword: [''],
    confirmPassword: [''],
  });
  private readonly pwValue = toSignal(this.password.valueChanges, {
    initialValue: this.password.getRawValue(),
  });
  protected readonly showNew = signal(false);
  protected readonly savingPassword = signal(false);
  protected readonly passwordError = signal<string | null>(null);

  protected readonly policyOk = computed(() =>
    isPasswordCompliant(this.pwValue().newPassword ?? ''),
  );
  protected readonly matches = computed(
    () => (this.pwValue().newPassword ?? '') === (this.pwValue().confirmPassword ?? ''),
  );
  protected readonly confirmTouched = computed(
    () => (this.pwValue().confirmPassword ?? '').length > 0,
  );
  protected readonly canChangePassword = computed(
    () =>
      !!this.pwValue().currentPassword &&
      this.policyOk() &&
      this.matches() &&
      this.confirmTouched(),
  );

  protected saveProfile(): void {
    if (this.profile.invalid) {
      this.profile.markAllAsTouched();
      return;
    }
    this.savingProfile.set(true);
    const v = this.profile.getRawValue();
    this.api
      .updateProfile({
        displayName: v.displayName.trim(),
        email: v.email.trim(),
        pronouns: v.pronouns.trim() || null,
        photoUrl: v.photoUrl.trim() || null,
      })
      .subscribe({
        next: () => {
          this.savingProfile.set(false);
          this.toasts.success('Profile updated.');
        },
        error: (err: unknown) => {
          this.savingProfile.set(false);
          this.toasts.error(ApiError.from(err).title);
        },
      });
  }

  protected changePassword(): void {
    this.passwordError.set(null);
    if (!this.canChangePassword()) return;
    this.savingPassword.set(true);
    const v = this.password.getRawValue();
    this.api
      .changePassword({
        currentPassword: v.currentPassword,
        newPassword: v.newPassword,
        confirmPassword: v.confirmPassword,
      })
      .subscribe({
        next: () => {
          this.savingPassword.set(false);
          this.password.reset();
          this.toasts.success('Password changed.');
        },
        error: (err: unknown) => {
          this.savingPassword.set(false);
          this.passwordError.set(ApiError.from(err).title);
        },
      });
  }
}
