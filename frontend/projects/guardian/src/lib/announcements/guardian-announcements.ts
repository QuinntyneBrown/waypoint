import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ApiError, GuardianAnnouncement, GuardianService } from '@waypoint/api';
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
} from '@waypoint/components';

// Traces to: L2-GUARDIAN-002
@Component({
  selector: 'w-guardian-announcements',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    Shell,
    DatePipe,
    Icon,
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
  ],
  templateUrl: './guardian-announcements.html',
})
export class GuardianAnnouncements {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(GuardianService);

  protected readonly announcements = signal<GuardianAnnouncement[] | undefined>(undefined);
  protected readonly error = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly unreadCount = computed(
    () => this.announcements()?.filter((announcement) => !announcement.read).length ?? 0,
  );

  constructor() {
    this.load();
  }

  protected markAllRead(): void {
    if (this.saving() || this.unreadCount() === 0) return;
    this.saving.set(true);
    this.api.markAllAnnouncementsRead().subscribe({
      next: () => {
        this.announcements.update((items) =>
          items?.map((announcement) => ({ ...announcement, read: true })),
        );
        this.saving.set(false);
      },
      error: (error: unknown) => {
        this.error.set(ApiError.from(error).title);
        this.saving.set(false);
      },
    });
  }

  private load(): void {
    this.api.announcements().subscribe({
      next: (announcements) => this.announcements.set(announcements),
      error: (error: unknown) => {
        this.announcements.set([]);
        this.error.set(ApiError.from(error).title);
      },
    });
  }
}
