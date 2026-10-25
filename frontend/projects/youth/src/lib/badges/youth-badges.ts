import { DatePipe, NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiError, LearningService, YouthJourney } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  Button,
  Shell,
  Alert,
  Card,
  CardBody,
  Badge,
  EmptyState,
  EmptyStateTitle,
  EmptyStateText,
  Progress,
} from '@waypoint/components';

// Traces to: L2-LEARN-009
@Component({
  selector: 'ph-youth-badges',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    Shell,
    RouterLink,
    DatePipe,
    NgTemplateOutlet,
    Button,
    Alert,
    Card,
    CardBody,
    Badge,
    EmptyState,
    EmptyStateTitle,
    EmptyStateText,
    Progress,
  ],
  templateUrl: './youth-badges.html',
  styleUrl: './youth-badges.css',
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
