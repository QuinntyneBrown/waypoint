import { DOCUMENT, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiError, MentorApi, MentorReviewPreview } from '@w/api';
import { AuthStore } from '@w/auth';
import { WIcon, WShell } from '@w/ui';

// Traces to: L2-MENTOR-001, L2-MENTOR-003, L2-MENTOR-004
@Component({
  selector: 'w-mentor-reviews',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, WIcon, RouterLink, DatePipe],
  templateUrl: './mentor-reviews.html',
  styles: `
    .queue-row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--w-s4);
      padding: var(--w-s4) 0;
      border-bottom: 1px solid var(--w-border);
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
  private readonly document = inject(DOCUMENT);
  protected readonly items = signal<MentorReviewPreview[] | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly statusMessage = signal<string | null>(null);
  protected readonly approvingAll = signal(false);
  protected readonly pendingQuizCount = computed(
    () => this.items()?.filter((item) => item.itemType === 'Quiz').length ?? 0,
  );

  constructor() {
    this.load();
  }

  protected approveAllQuizzes(): void {
    const count = this.pendingQuizCount();
    if (
      count === 0 ||
      this.approvingAll() ||
      !this.document.defaultView?.confirm(
        `Approve all ${count} pending quiz assessment${count === 1 ? '' : 's'} at the auto-scored result?`,
      )
    ) {
      return;
    }

    this.approvingAll.set(true);
    this.errorMessage.set(null);
    this.api.approveAllQuizAttempts().subscribe({
      next: (approved) => {
        this.approvingAll.set(false);
        this.statusMessage.set(`Approved ${approved} quiz assessment${approved === 1 ? '' : 's'}.`);
        this.load();
      },
      error: (error: unknown) => {
        this.approvingAll.set(false);
        this.errorMessage.set(ApiError.from(error).title);
      },
    });
  }

  private load(): void {
    this.api.reviews(100).subscribe({
      next: (items) => this.items.set(items),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }
}
