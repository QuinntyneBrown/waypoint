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
  WButton,
  WIcon,
  WShell,
  WAlert,
  WCard,
  WCardBody,
  WCardTitle,
  WBadge,
  WEmptyState,
  WEmptyStateIcon,
  WEmptyStateTitle,
  WEmptyStateText,
} from '@waypoint/components';
import { forkJoin } from 'rxjs';

// Traces to: L2-MENTOR-001, L2-AUTH-010
@Component({
  selector: 'w-mentor-dashboard',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    WShell,
    WIcon,
    RouterLink,
    DatePipe,
    WButton,
    WAlert,
    WCard,
    WCardBody,
    WCardTitle,
    WBadge,
    WEmptyState,
    WEmptyStateIcon,
    WEmptyStateTitle,
    WEmptyStateText,
  ],
  templateUrl: './mentor-dashboard.html',
  styles: `
    .mentor-preview-grid {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: var(--w-s4);
    }
    .youth-preview {
      display: grid;
      grid-template-columns: auto minmax(0, 1fr);
      align-items: center;
      gap: var(--w-s4);
    }
    .progress-number {
      display: grid;
      place-items: center;
      width: 3.5rem;
      height: 3.5rem;
      border: 0.35rem solid var(--w-lime);
      border-radius: 50%;
      font-weight: 700;
    }
    .review-row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--w-s4);
      padding: var(--w-s4) 0;
      border-bottom: 1px solid var(--w-border);
    }
    .review-row:last-child {
      border-bottom: 0;
    }
    .review-link {
      min-height: 44px;
      flex: none;
    }
    @media (max-width: 48rem) {
      .mentor-preview-grid {
        grid-template-columns: 1fr;
      }
      .review-row {
        align-items: flex-start;
        flex-direction: column;
      }
      .review-link {
        width: 100%;
      }
    }
  `,
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
