import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { VolunteerService, VolunteerScheduleItem } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WIcon, WShell, WCard, WCardBody, WCardTitle, WBadge } from '@waypoint/components';

// Traces to: L2-VOLUNTEER-002
@Component({
  selector: 'w-volunteer-schedule',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, DatePipe, WIcon, WCard, WCardBody, WCardTitle, WBadge],
  templateUrl: './volunteer-schedule.html',
})
export class VolunteerSchedule {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(VolunteerService);

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
