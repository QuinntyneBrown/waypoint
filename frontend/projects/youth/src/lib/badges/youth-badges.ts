import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiError, LearningService, YouthJourney } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WButton, WShell, WAlert } from '@waypoint/components';

// Traces to: L2-LEARN-009
@Component({
  selector: 'w-youth-badges',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, RouterLink, DatePipe, WButton, WAlert],
  templateUrl: './youth-badges.html',
})
export class YouthBadgesPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(LearningService);
  protected readonly journey = signal<YouthJourney | undefined>(undefined);
  protected readonly errorMessage = signal<string | null>(null);

  constructor() {
    this.api.journey().subscribe({
      next: (journey) => this.journey.set(journey),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }
}
