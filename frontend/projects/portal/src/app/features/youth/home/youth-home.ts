import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { OnboardingApi } from '@wu/api';
import { AuthStore } from '@wu/auth';
import { WuShell } from '@wu/ui';

// Traces to: L2-PLATFORM-011
@Component({
  selector: 'wu-youth-home',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, RouterLink],
  templateUrl: './youth-home.html',
})
export class YouthHome {
  protected readonly auth = inject(AuthStore);
  private readonly onboarding = inject(OnboardingApi);

  protected readonly firstName = computed(
    () => (this.auth.user()?.displayName ?? '').split(/\s+/)[0] || 'friend',
  );
  /** null = still loading; true = placed in a cohort; false = no cohort yet. */
  protected readonly placed = signal<boolean | null>(null);

  constructor() {
    this.onboarding.status().subscribe({
      next: (status) => this.placed.set(status.cohortId !== null),
      error: () => this.placed.set(false),
    });
  }
}
