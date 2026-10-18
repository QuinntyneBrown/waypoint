import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { VolunteerService, VolunteerTodayView } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  Button,
  Icon,
  Shell,
  Card,
  CardBody,
  CardTitle,
  EmptyState,
  EmptyStateIcon,
  EmptyStateTitle,
  EmptyStateText,
} from '@waypoint/components';

// Traces to: L2-VOLUNTEER-001
@Component({
  selector: 'w-volunteer-today',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    Shell,
    RouterLink,
    DatePipe,
    Icon,
    Button,
    Card,
    CardBody,
    CardTitle,
    EmptyState,
    EmptyStateIcon,
    EmptyStateTitle,
    EmptyStateText,
  ],
  templateUrl: './volunteer-today.html',
  styleUrl: './volunteer-today.css',
})
export class VolunteerToday {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(VolunteerService);

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
