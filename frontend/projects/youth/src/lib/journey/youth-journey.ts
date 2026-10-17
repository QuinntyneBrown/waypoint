import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiError, LearningService, YouthJourney } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WButton, WShell, WAlert, WCard, WCardBody } from '@waypoint/components';

// Traces to: L2-LEARN-009
@Component({
  selector: 'w-youth-journey',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, RouterLink, WButton, WAlert, WCard, WCardBody],
  templateUrl: './youth-journey.html',
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
