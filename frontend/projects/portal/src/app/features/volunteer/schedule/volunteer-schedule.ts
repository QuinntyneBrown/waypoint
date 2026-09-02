import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { VolunteerApi, VolunteerScheduleItem } from '@w/api';
import { AuthStore } from '@w/auth';
import { WShell } from '@w/ui';

// Traces to: L2-VOLUNTEER-002
@Component({
  selector: 'w-volunteer-schedule',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, DatePipe],
  templateUrl: './volunteer-schedule.html',
})
export class VolunteerSchedule {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(VolunteerApi);

  /** undefined = still loading; array (possibly empty) = loaded. */
  protected readonly sessions = signal<VolunteerScheduleItem[] | undefined>(undefined);
  protected readonly loading = computed(() => this.sessions() === undefined);

  constructor() {
    this.api.schedule().subscribe({
      next: (items) => this.sessions.set(items),
      error: () => this.sessions.set([]),
    });
  }
}
