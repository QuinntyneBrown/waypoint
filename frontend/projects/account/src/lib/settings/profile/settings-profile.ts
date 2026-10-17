import { DOCUMENT } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnDestroy,
  signal,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  ApiError,
  isPasswordCompliant,
  PASSWORD_POLICY_MESSAGE,
  ProfileService,
} from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  ToastService,
  WButton,
  WErrorText,
  WFormField,
  WHelp,
  WIcon,
  WIconButton,
  WLabel,
  WLabelRequired,
  WTextField,
} from '@waypoint/components';
import { SettingsShell } from '../settings-shell';

// Traces to: L2-PLATFORM-002
@Component({
  selector: 'w-settings-profile',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    SettingsShell,
    ReactiveFormsModule,
    WIcon,
    WButton,
    WIconButton,
    WFormField,
    WLabel,
    WLabelRequired,
    WHelp,
    WErrorText,
    WTextField,
  ],
  templateUrl: './settings-profile.html',
})
export class SettingsProfile implements OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ProfileService);
  private readonly auth = inject(AuthStore);
  private readonly toasts = inject(ToastService);
  private readonly document = inject(DOCUMENT);
  private photoObjectUrl: string | null = null;

  protected readonly policyMessage = PASSWORD_POLICY_MESSAGE;

  protected readonly profile = this.fb.nonNullable.group({
    displayName: [this.auth.user()?.displayName ?? '', [Validators.required]],
    email: [this.auth.user()?.email ?? '', [Validators.required, Validators.email]],
    pronouns: [''],
  });
  protected readonly savingProfile = signal(false);
  protected readonly uploadingPhoto = signal(false);
  protected readonly removingPhoto = signal(false);
  protected readonly hasPhoto = signal(false);
  protected readonly photoPreview = signal<string | null>(null);
  protected readonly photoError = signal<string | null>(null);
  protected readonly photoStatus = signal<string | null>(null);

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

  constructor() {
    this.api.getProfile().subscribe({
      next: (profile) => {
        this.profile.patchValue({
          displayName: profile.displayName,
          email: profile.email,
          pronouns: profile.pronouns ?? '',
        });
        this.hasPhoto.set(profile.hasPhoto);
        if (profile.photoUrl) this.loadPhoto(profile.photoUrl);
      },
    });
  }

  ngOnDestroy(): void {
    this.revokePhotoPreview();
  }

  protected photoSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    const extension = file.name.split('.').pop()?.toLowerCase();
    if (
      !['image/png', 'image/jpeg'].includes(file.type) ||
      !['png', 'jpg', 'jpeg'].includes(extension ?? '')
    ) {
      this.photoError.set('Choose a JPG or PNG image.');
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.photoError.set('Profile photo must be 5 MB or smaller.');
      return;
    }

    this.photoError.set(null);
    this.photoStatus.set('Uploading profile photo…');
    this.uploadingPhoto.set(true);
    this.api.uploadPhoto(file).subscribe({
      next: () => {
        this.uploadingPhoto.set(false);
        this.hasPhoto.set(true);
        this.photoStatus.set('Profile photo ready.');
        this.showPhotoBlob(file);
      },
      error: (error: unknown) => {
        this.uploadingPhoto.set(false);
        this.photoStatus.set(null);
        const apiError = ApiError.from(error);
        this.photoError.set(apiError.fieldErrors[0]?.errorMessage ?? apiError.title);
      },
    });
  }

  protected removePhoto(): void {
    if (this.removingPhoto()) return;
    this.removingPhoto.set(true);
    this.photoError.set(null);
    this.api.removePhoto().subscribe({
      next: () => {
        this.removingPhoto.set(false);
        this.hasPhoto.set(false);
        this.photoStatus.set('Profile photo removed.');
        this.revokePhotoPreview();
        this.photoPreview.set(null);
      },
      error: (error: unknown) => {
        this.removingPhoto.set(false);
        this.photoError.set(ApiError.from(error).title);
      },
    });
  }

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

  private loadPhoto(path: string): void {
    this.api.photo(path).subscribe({
      next: (blob) => this.showPhotoBlob(blob),
      error: (error: unknown) => this.photoError.set(ApiError.from(error).title),
    });
  }

  private showPhotoBlob(blob: Blob): void {
    const window = this.document.defaultView;
    if (!window) return;
    this.revokePhotoPreview();
    this.photoObjectUrl = window.URL.createObjectURL(blob);
    this.photoPreview.set(this.photoObjectUrl);
  }

  private revokePhotoPreview(): void {
    if (this.photoObjectUrl) {
      this.document.defaultView?.URL.revokeObjectURL(this.photoObjectUrl);
      this.photoObjectUrl = null;
    }
  }
}
