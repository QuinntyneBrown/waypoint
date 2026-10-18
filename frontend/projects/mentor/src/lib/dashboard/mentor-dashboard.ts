import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  ApiError,
  MentorService,
  MentorDashboardView,
  MentorReviewPreview,
  MentorRosterYouth,
} from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  Button,
  Icon,
  Shell,
  Alert,
  Card,
  CardBody,
  CardTitle,
  Badge,
  EmptyState,
  EmptyStateIcon,
  EmptyStateTitle,
  EmptyStateText,
  Stat,
  StatLabel,
  StatValue,
  StatDelta,
} from '@waypoint/components';
import { forkJoin } from 'rxjs';

// Traces to: L2-MENTOR-001, L2-AUTH-010
@Component({
  selector: 'w-mentor-dashboard',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    Shell,
    Icon,
    RouterLink,
    DatePipe,
    Button,
    Alert,
    Card,
    CardBody,
    CardTitle,
    Badge,
    EmptyState,
    EmptyStateIcon,
    EmptyStateTitle,
    EmptyStateText,
    Stat,
    StatLabel,
    StatValue,
    StatDelta,
  ],
  templateUrl: './mentor-dashboard.html',
  styleUrl: './mentor-dashboard.css',
})
export class MentorDashboardPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(MentorService);

  protected readonly summary = signal<MentorDashboardView | null>(null);
  protected readonly youth = signal<MentorRosterYouth[]>([]);
  protected readonly reviews = signal<MentorReviewPreview[]>([]);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);

  constructor() {
    forkJoin({
      summary: this.api.dashboard(),
      youth: this.api.roster('all'),
      reviews: this.api.reviews(5),
    }).subscribe({
      next: ({ summary, youth, reviews }) => {
        this.summary.set(summary);
        this.youth.set(youth);
        this.reviews.set(reviews);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.errorMessage.set(ApiError.from(error).title);
        this.loading.set(false);
      },
    });
  }

  protected percent(value: number): number {
    return Math.round(value * 100);
  }
}
