import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { AnnouncementsApi, AnnouncementView } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WIcon, WShell } from '@waypoint/components';
import { shellRole } from '@waypoint/core';

// Traces to: L2-ENGAGE-006
@Component({
  selector: 'w-youth-announcements',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, WIcon],
  templateUrl: './youth-announcements.html',
})
export class YouthAnnouncements {
  private readonly api = inject(AnnouncementsApi);
  private readonly auth = inject(AuthStore);

  protected readonly role = computed(() => shellRole(this.auth.role()));
  protected readonly userName = computed(() => this.auth.user()?.displayName ?? '');
  protected readonly items = signal<AnnouncementView[]>([]);
  protected readonly loading = signal(true);
  protected readonly hasUnread = computed(() => this.items().some((i) => !i.read));

  constructor() {
    this.api.mine().subscribe({
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
