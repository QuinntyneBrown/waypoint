import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import {
  NOTIFICATION_CATEGORIES,
  NOTIFICATION_DEFAULTS,
  NotificationPreferencesApi,
  OnboardingApi,
} from '@wu/api';
import { AuthStore } from '@wu/auth';
import { WuIcon } from '@wu/ui';
import { forkJoin, of, switchMap } from 'rxjs';

// Traces to: L2-AUTH-011
@Component({
  selector: 'wu-onboarding',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, WuIcon],
  templateUrl: './onboarding.html',
})
export class Onboarding {
  private readonly fb = inject(FormBuilder);
  private readonly onboarding = inject(OnboardingApi);
  private readonly notifications = inject(NotificationPreferencesApi);
  private readonly auth = inject(AuthStore);
  private readonly router = inject(Router);

  protected readonly categories = NOTIFICATION_CATEGORIES;
  protected readonly step = signal(1);
  protected readonly saving = signal(false);
  protected readonly placed = signal<boolean | null>(null);

  protected readonly profile = this.fb.nonNullable.group({
    displayName: ['', [Validators.required, Validators.maxLength(128)]],
    pronouns: ['', [Validators.maxLength(64)]],
    photoUrl: ['', [Validators.maxLength(512)]],
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
    const { displayName, pronouns, photoUrl } = this.profile.getRawValue();
    const saves = this.categories.map((c) =>
      this.notifications.save({
        eventKey: c.eventKey,
        inApp: this.prefs()[c.eventKey],
        email: false,
      }),
    );
    // Persist notification choices, then mark onboarding complete.
    (saves.length ? forkJoin(saves) : of([]))
      .pipe(switchMap(() => this.onboarding.complete({ displayName, pronouns, photoUrl })))
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
}
