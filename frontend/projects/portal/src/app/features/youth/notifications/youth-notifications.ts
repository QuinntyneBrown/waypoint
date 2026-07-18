import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { NotificationsApi, NotificationView } from '@wu/api';
import { AuthStore } from '@wu/auth';
import { WuIcon, WuShell } from '@wu/ui';
import { shellRole } from '../../../shell-role';

// Traces to: L2-ENGAGE-008
@Component({
  selector: 'wu-youth-notifications',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, WuIcon],
  templateUrl: './youth-notifications.html',
})
export class YouthNotifications {
  private readonly api = inject(NotificationsApi);
  private readonly auth = inject(AuthStore);

  protected readonly role = computed(() => shellRole(this.auth.role()));
  protected readonly userName = computed(() => this.auth.user()?.displayName ?? '');
  protected readonly items = signal<NotificationView[]>([]);
  protected readonly loading = signal(true);
  protected readonly hasUnread = computed(() => this.items().some((i) => !i.read));

  constructor() {
    this.api.feed().subscribe({
      next: (list) => {
        this.items.set(list);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
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
