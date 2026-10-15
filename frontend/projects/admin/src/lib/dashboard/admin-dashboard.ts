import { DatePipe, PercentPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiError, ReportsApi, ReportsHub } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WIcon, WShell } from '@waypoint/components';

// Traces to: L2-ADMIN-004, L2-REPORT-001
@Component({
  selector: 'w-admin-dashboard',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, PercentPipe, RouterLink, WIcon, WShell],
  templateUrl: './admin-dashboard.html',
  styles: `
    .dashboard-grid {
      display: grid;
      gap: var(--w-s5);
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }
    .dashboard-row {
      border-top: 1px solid var(--w-border);
      padding: var(--w-s4) 0;
    }
    .dashboard-row:first-child {
      border-top: 0;
    }
    .dashboard-actions {
      display: grid;
      gap: var(--w-s3);
      grid-template-columns: repeat(3, minmax(0, 1fr));
    }
    .dashboard-action {
      align-items: center;
      display: flex;
      gap: var(--w-s3);
      min-height: 56px;
    }
    @media (max-width: 62rem) {
      .dashboard-actions {
        grid-template-columns: repeat(2, minmax(0, 1fr));
      }
    }
    @media (max-width: 48rem) {
      .dashboard-grid,
      .dashboard-actions {
        grid-template-columns: 1fr;
      }
    }
  `,
})
export class AdminDashboardPage {
  protected readonly auth = inject(AuthStore);
  private readonly reports = inject(ReportsApi);
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
