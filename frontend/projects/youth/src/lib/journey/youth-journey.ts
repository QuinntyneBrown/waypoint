import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiError, LearningService, YouthJourney } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WShell } from '@waypoint/components';

// Traces to: L2-LEARN-009
@Component({
  selector: 'w-youth-journey',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, RouterLink],
  templateUrl: './youth-journey.html',
  styles: `
    .journey-timeline {
      border-inline-start: 3px solid var(--w-border);
      padding-inline-start: var(--w-s5);
    }
    .journey-module {
      position: relative;
    }
    .journey-module::before {
      content: '';
      position: absolute;
      inline-size: 0.9rem;
      block-size: 0.9rem;
      border-radius: 50%;
      background: var(--w-border);
      inset-inline-start: calc(-1 * var(--w-s5) - 0.55rem);
      inset-block-start: 1.35rem;
    }
    .journey-module--current::before {
      background: var(--w-lime);
      box-shadow: 0 0 0 4px var(--w-lime-soft);
    }
    .journey-progress {
      width: 100%;
    }
  `,
})
export class YouthJourneyPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(LearningService);
  protected readonly journey = signal<YouthJourney | undefined>(undefined);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly completedModules = computed(
    () => this.journey()?.modules.filter((module) => module.status === 'done').length ?? 0,
  );
  protected readonly currentModule = computed(() =>
    this.journey()?.modules.find((module) => module.current),
  );
  protected readonly overallPercent = computed(() => {
    const modules = this.journey()?.modules ?? [];
    const total = modules.reduce((sum, module) => sum + module.totalItems, 0);
    const completed = modules.reduce((sum, module) => sum + module.completedItems, 0);
    return total === 0 ? 0 : Math.round((completed / total) * 100);
  });

  constructor() {
    this.api.journey().subscribe({
      next: (journey) => this.journey.set(journey),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  protected statusLabel(status: string): string {
    if (status === 'in-progress') return 'In progress';
    if (status === 'not-started') return 'Not started';
    if (status === 'done') return 'Done';
    return 'Locked';
  }
}
