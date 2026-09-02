import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { VolunteerApi, VolunteerTodayView } from '@w/api';
import { AuthStore } from '@w/auth';
import { WShell } from '@w/ui';

// Traces to: L2-VOLUNTEER-001
@Component({
  selector: 'w-volunteer-today',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, RouterLink, DatePipe],
  templateUrl: './volunteer-today.html',
})
export class VolunteerToday {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(VolunteerApi);

  /** undefined = still loading; null = no session today; object = today's session. */
  protected readonly today = signal<VolunteerTodayView | null | undefined>(undefined);
  protected readonly loading = computed(() => this.today() === undefined);

  constructor() {
    this.api.today().subscribe({
      next: (view) => this.today.set(view ?? null),
      error: () => this.today.set(null),
    });
  }
}
