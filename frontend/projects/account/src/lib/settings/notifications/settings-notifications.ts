import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import {
  NOTIFICATION_CATEGORIES,
  NOTIFICATION_DEFAULTS,
  NotificationPreferencesService,
} from '@waypoint/api';
import { ToastService } from '@waypoint/components';
import { SettingsShell } from '../settings-shell';

type Channel = 'inApp' | 'email';
interface PrefRow {
  inApp: boolean;
  email: boolean;
}

// Traces to: L2-PLATFORM-003
@Component({
  selector: 'w-settings-notifications',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [SettingsShell],
  templateUrl: './settings-notifications.html',
})
export class SettingsNotifications {
  private readonly api = inject(NotificationPreferencesService);
  private readonly toasts = inject(ToastService);

  protected readonly categories = NOTIFICATION_CATEGORIES;
  protected readonly prefs = signal<Record<string, PrefRow>>(this.seedDefaults());

  constructor() {
    // Overlay any persisted rows onto the defaults.
    this.api.list().subscribe((rows) => {
      const next = { ...this.prefs() };
      for (const row of rows) next[row.eventKey] = { inApp: row.inApp, email: row.email };
      this.prefs.set(next);
    });
  }

  private seedDefaults(): Record<string, PrefRow> {
    const seed: Record<string, PrefRow> = {};
    for (const c of NOTIFICATION_CATEGORIES) {
      seed[c.eventKey] = { inApp: NOTIFICATION_DEFAULTS[c.eventKey] ?? false, email: false };
    }
    return seed;
  }

  protected toggle(eventKey: string, channel: Channel): void {
    const current = this.prefs()[eventKey];
    const next: PrefRow = { ...current, [channel]: !current[channel] };
    this.prefs.update((p) => ({ ...p, [eventKey]: next }));
    this.api.save({ eventKey, inApp: next.inApp, email: next.email }).subscribe({
      next: () => this.toasts.success('Notification preferences saved.'),
      error: () => this.toasts.error('Couldn’t save that change.'),
    });
  }
}
