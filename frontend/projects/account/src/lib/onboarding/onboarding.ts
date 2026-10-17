import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnDestroy, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import {
  NOTIFICATION_CATEGORIES,
  NOTIFICATION_DEFAULTS,
  NotificationPreferencesService,
  OnboardingService,
  ProfileService,
  ApiError,
} from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  WButton,
  WErrorText,
  WFormField,
  WHelp,
  WIcon,
  WLabel,
  WLabelRequired,
  WTextField,
  WSwitch,
  WSwitchTrack,
} from '@waypoint/components';
import { forkJoin, of, switchMap } from 'rxjs';

// Traces to: L2-AUTH-011
@Component({
  selector: 'w-onboarding',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    WIcon,
    WButton,
    WFormField,
    WLabel,
    WLabelRequired,
    WHelp,
    WErrorText,
    WTextField,
    WSwitch,
    WSwitchTrack,
  ],
  templateUrl: './onboarding.html',
})
export class Onboarding implements OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly onboarding = inject(OnboardingService);
  private readonly notifications = inject(NotificationPreferencesService);
  private readonly auth = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly profileApi = inject(ProfileService);
  private readonly document = inject(DOCUMENT);
  private photoObjectUrl: string | null = null;

  protected readonly categories = NOTIFICATION_CATEGORIES;
  protected readonly step = signal(1);
  protected readonly saving = signal(false);
  protected readonly placed = signal<boolean | null>(null);
  protected readonly uploadingPhoto = signal(false);
  protected readonly photoPreview = signal<string | null>(null);
  protected readonly photoError = signal<string | null>(null);
  protected readonly photoStatus = signal<string | null>(null);

  protected readonly profile = this.fb.nonNullable.group({
    displayName: ['', [Validators.required, Validators.maxLength(128)]],
    pronouns: ['', [Validators.maxLength(64)]],
  });

  /** In-app on/off per category; seeded from the onboarding defaults (ON/ON/OFF). */
  protected readonly prefs = signal<Record<string, boolean>>({ ...NOTIFICATION_DEFAULTS });

  constructor() {
    this.profile.controls.displayName.setValue(this.auth.user()?.displayName ?? '');
    this.onboarding.status().subscribe({
      next: (status) => {
        if (status.displayName) this.profile.controls.displayName.setValue(status.displayName);
        if (status.pronouns) this.profile.controls.pronouns.setValue(status.pronouns);
        this.placed.set(status.cohortId !== null);
      },
      error: () => this.placed.set(false),
    });
    this.profileApi.getProfile().subscribe({
      next: (profile) => {
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

    this.uploadingPhoto.set(true);
    this.photoError.set(null);
    this.photoStatus.set('Uploading profile photo…');
    this.profileApi.uploadPhoto(file).subscribe({
      next: () => {
        this.uploadingPhoto.set(false);
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

  protected toggle(eventKey: string): void {
    this.prefs.update((p) => ({ ...p, [eventKey]: !p[eventKey] }));
  }

  protected next(): void {
    if (this.step() === 1 && this.profile.controls.displayName.invalid) {
      this.profile.controls.displayName.markAsTouched();
      return;
    }
    this.step.update((s) => Math.min(s + 1, 3));
  }

  protected back(): void {
    this.step.update((s) => Math.max(s - 1, 1));
  }

  protected finish(): void {
    if (this.profile.invalid) {
      this.step.set(1);
      this.profile.markAllAsTouched();
      return;
    }
    this.saving.set(true);
    const { displayName, pronouns } = this.profile.getRawValue();
    const saves = this.categories.map((c) =>
      this.notifications.save({
        eventKey: c.eventKey,
        inApp: this.prefs()[c.eventKey],
        email: false,
      }),
    );
    // Persist notification choices, then mark onboarding complete.
    (saves.length ? forkJoin(saves) : of([]))
      .pipe(switchMap(() => this.onboarding.complete({ displayName, pronouns })))
      .subscribe({
        next: () => void this.router.navigateByUrl('/youth/home'),
        error: () => {
          this.saving.set(false);
          void this.router.navigateByUrl('/youth/home');
        },
      });
  }

  protected skip(): void {
    this.saving.set(true);
    this.onboarding.skip().subscribe({
      next: () => void this.router.navigateByUrl('/youth/home'),
      error: () => void this.router.navigateByUrl('/youth/home'),
    });
  }

  private loadPhoto(path: string): void {
    this.profileApi.photo(path).subscribe({
      next: (blob) => this.showPhotoBlob(blob),
      error: () => undefined,
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
