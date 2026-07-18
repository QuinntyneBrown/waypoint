import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiError, LearningApi, YouthJourney } from '@wu/api';
import { AuthStore } from '@wu/auth';
import { WuShell } from '@wu/ui';

// Traces to: L2-LEARN-009
@Component({
  selector: 'wu-youth-badges',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, RouterLink, DatePipe],
  templateUrl: './youth-badges.html',
})
export class YouthBadgesPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(LearningApi);
  protected readonly journey = signal<YouthJourney | undefined>(undefined);
  protected readonly errorMessage = signal<string | null>(null);

  constructor() {
    this.api.journey().subscribe({
      next: (journey) => this.journey.set(journey),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }
}
