import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiError, MentorApi, MentorReviewPreview } from '@wu/api';
import { AuthStore } from '@wu/auth';
import { WuIcon, WuShell } from '@wu/ui';

// Traces to: L2-MENTOR-001, L2-MENTOR-003, L2-MENTOR-004
@Component({
  selector: 'wu-mentor-reviews',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, WuIcon, RouterLink, DatePipe],
  templateUrl: './mentor-reviews.html',
  styles: `
    .queue-row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--wu-s4);
      padding: var(--wu-s4) 0;
      border-bottom: 1px solid var(--wu-border);
    }
    .queue-row:last-child {
      border-bottom: 0;
    }
    .queue-link {
      min-height: 44px;
      flex: none;
    }
    @media (max-width: 40rem) {
      .queue-row {
        align-items: flex-start;
        flex-direction: column;
      }
      .queue-link {
        width: 100%;
      }
    }
  `,
})
export class MentorReviewsPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(MentorApi);
  protected readonly items = signal<MentorReviewPreview[] | null>(null);
  protected readonly errorMessage = signal<string | null>(null);

  constructor() {
    this.api.reviews(100).subscribe({
      next: (items) => this.items.set(items),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }
}
