import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiError, LearningService, YouthHomeView } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WIcon, WShell } from '@waypoint/components';

// Traces to: L2-LEARN-001..010, L2-ENGAGE-005, L2-MENTOR-008, L2-PLATFORM-011
@Component({
  selector: 'w-youth-home',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, RouterLink, DatePipe, DecimalPipe, WIcon],
  templateUrl: './youth-home.html',
  styles: `
    .home-hero {
      display: grid;
      grid-template-columns: minmax(0, 1fr) auto;
      gap: var(--w-s5);
      align-items: center;
    }
    .home-progress {
      width: 100%;
    }
    .home-progress__value {
      font-size: clamp(2rem, 7vw, 3.25rem);
      font-weight: 800;
      line-height: 1;
    }
    .assignment-list {
      margin: 0;
      padding: 0;
      list-style: none;
    }
    .assignment-row {
      display: grid;
      grid-template-columns: minmax(0, 1fr) auto;
      gap: var(--w-s4);
      align-items: center;
      padding-block: var(--w-s4);
      border-top: 1px solid var(--w-border);
    }
    .assignment-row:first-child {
      border-top: 0;
    }
    @media (max-width: 42rem) {
      .home-hero,
      .assignment-row {
        grid-template-columns: minmax(0, 1fr);
      }
      .assignment-row .w-btn {
        width: 100%;
      }
    }
  `,
})
export class YouthHome {
  protected readonly auth = inject(AuthStore);
  private readonly learning = inject(LearningService);

  protected readonly firstName = computed(
    () => (this.auth.user()?.displayName ?? '').split(/\s+/)[0] || 'friend',
  );
  protected readonly home = signal<YouthHomeView | undefined>(undefined);
  protected readonly errorMessage = signal<string | null>(null);

  constructor() {
    this.learning.home().subscribe({
      next: (home) => this.home.set(home),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }
}
