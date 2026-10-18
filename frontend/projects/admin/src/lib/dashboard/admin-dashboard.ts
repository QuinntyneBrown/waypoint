import { DatePipe, PercentPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiError, ReportsService, ReportsHub } from '@waypoint/api';
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
  Stat,
  StatLabel,
  StatValue,
} from '@waypoint/components';

// Traces to: L2-ADMIN-004, L2-REPORT-001
@Component({
  selector: 'w-admin-dashboard',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    PercentPipe,
    RouterLink,
    Icon,
    Shell,
    Button,
    Alert,
    Card,
    CardBody,
    CardTitle,
    Badge,
    Stat,
    StatLabel,
    StatValue,
  ],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css',
})
export class AdminDashboardPage {
  protected readonly auth = inject(AuthStore);
  private readonly reports = inject(ReportsService);
  protected readonly hub = signal<ReportsHub | null>(null);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.reports.hub().subscribe({
      next: (hub) => {
        this.hub.set(hub);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.errorMessage.set(ApiError.from(error).title);
        this.loading.set(false);
      },
    });
  }
}
