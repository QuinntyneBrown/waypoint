import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { ApiError, NotificationsService, NotificationView } from '@waypoint/api';
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
import { shellRole } from '@waypoint/core';
import { RealtimeService } from '@waypoint/core/realtime';

// Traces to: L2-ENGAGE-008, L2-NFR-005
@Component({
  selector: 'w-notifications',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    Shell,
    Icon,
    RouterLink,
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
  templateUrl: './youth-notifications.html',
  styleUrl: './youth-notifications.css',
})
export class YouthNotifications {
  private readonly api = inject(NotificationsService);
  private readonly auth = inject(AuthStore);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly realtime = inject(RealtimeService);

  protected readonly role = computed(() => shellRole(this.auth.role()));
  protected readonly userName = computed(() => this.auth.user()?.displayName ?? '');
  protected readonly items = signal<NotificationView[]>([]);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly liveStatus = signal<string | null>(null);
  protected readonly hasUnread = computed(() => this.items().some((i) => !i.read));

  constructor() {
    this.load();
    this.realtime.notifications$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((notification) => {
        this.items.update((current) => this.merge(current, [notification], false));
        this.liveStatus.set(`New notification: ${notification.title}.`);
      });
    this.realtime.reconnected$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.liveStatus.set('Live updates reconnected.');
      this.load(false);
    });
  }

  private load(showLoading = true): void {
    if (showLoading) this.loading.set(true);
    this.errorMessage.set(null);
    this.api.feed().subscribe({
      next: (list) => {
        this.items.update((current) => this.merge(current, list, true));
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.errorMessage.set(ApiError.from(error).title);
        this.loading.set(false);
      },
    });
  }

  /**
   * Reconciles a feed response with events that may have arrived while it was in flight.
   * Feed records win for existing IDs because they contain the authoritative read state.
   */
  private merge(
    current: NotificationView[],
    incoming: NotificationView[],
    incomingIsAuthoritative: boolean,
  ): NotificationView[] {
    const byId = new Map(current.map((item) => [item.id, item]));
    for (const item of incoming) {
      if (incomingIsAuthoritative || !byId.has(item.id)) byId.set(item.id, item);
    }
    return [...byId.values()].sort(
      (left, right) => Date.parse(right.createdAt) - Date.parse(left.createdAt),
    );
  }

  protected markAllRead(): void {
    this.api
      .markAllRead()
      .subscribe(() => this.items.update((list) => list.map((i) => ({ ...i, read: true }))));
  }

  protected signOut(): void {
    this.auth.signOut();
  }
}
