import { DOCUMENT, DatePipe, PercentPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  ApiError,
  SessionAttendanceSummary,
  SessionSchedulingOptions,
  StaffOperationsApi,
  YouthAttendanceSummary,
} from '@wu/api';
import { AuthStore } from '@wu/auth';
import { WuRole, WuShell } from '@wu/ui';
import { forkJoin } from 'rxjs';

type HistoryView = 'session' | 'youth';

// Traces to: L2-ENGAGE-004, L2-REPORT-003, L2-NFR-009/010
@Component({
  selector: 'wu-mentor-attendance-history',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, DatePipe, PercentPipe, RouterLink],
  templateUrl: './mentor-attendance-history.html',
  styles: `
    .history-toolbar {
      display: flex;
      flex-wrap: wrap;
      align-items: end;
      justify-content: space-between;
      gap: var(--wu-s4);
    }
    .history-filters {
      display: flex;
      flex-wrap: wrap;
      align-items: end;
      gap: var(--wu-s3);
    }
    .history-table-wrap {
      max-width: 100%;
      overflow-x: auto;
    }
    .history-table {
      width: 100%;
      min-width: 52rem;
      border-collapse: collapse;
    }
    .history-table th,
    .history-table td {
      padding: var(--wu-s3);
      border-bottom: 1px solid var(--wu-color-border);
      text-align: left;
      vertical-align: middle;
    }
    [role='tab'],
    .history-toolbar .wu-btn,
    .history-table .wu-btn {
      min-height: 44px;
    }
    @media (max-width: 42rem) {
      .history-filters,
      .history-filters .wu-field,
      .history-filters .wu-btn,
      .history-toolbar > .wu-cluster {
        width: 100%;
      }
      .history-toolbar > .wu-cluster .wu-btn {
        flex: 1;
      }
    }
  `,
})
export class MentorAttendanceHistoryPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(StaffOperationsApi);
  private readonly doc = inject(DOCUMENT);

  protected readonly options = signal<SessionSchedulingOptions | null>(null);
  protected readonly view = signal<HistoryView>('session');
  protected readonly cohortId = signal('');
  protected readonly from = signal(this.daysAgo(90));
  protected readonly to = signal(this.daysAgo(-1));
  protected readonly sessions = signal<SessionAttendanceSummary[]>([]);
  protected readonly youth = signal<YouthAttendanceSummary[]>([]);
  protected readonly loading = signal(true);
  protected readonly exporting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly statusMessage = signal<string | null>(null);
  protected readonly isAdmin = computed(() => this.auth.user()?.role === 'ProgramAdministrator');
  protected readonly shellRole = computed<WuRole>(() => (this.isAdmin() ? 'admin' : 'mentor'));
  protected readonly calendarPath = computed(() =>
    this.isAdmin() ? '/admin/calendar' : '/mentor/calendar',
  );
  protected readonly attendanceBase = computed(() =>
    this.isAdmin() ? '/admin/attendance' : '/mentor/attendance',
  );

  constructor() {
    this.api.sessionOptions().subscribe({
      next: (options) => {
        this.options.set(options);
        this.load();
      },
      error: (error: unknown) => this.fail(error),
    });
  }

  protected setView(view: HistoryView): void {
    this.view.set(view);
  }

  protected load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    const filter = this.filter();
    forkJoin({
      sessions: this.api.sessionHistory(filter),
      youth: this.api.youthHistory(filter),
    }).subscribe({
      next: ({ sessions, youth }) => {
        this.sessions.set(sessions);
        this.youth.set(youth);
        this.loading.set(false);
      },
      error: (error: unknown) => this.fail(error),
    });
  }

  protected export(format: 'csv' | 'pdf'): void {
    if (this.exporting()) return;
    this.exporting.set(true);
    this.errorMessage.set(null);
    const view = this.view();
    this.api.exportAttendance(format, view, this.filter()).subscribe({
      next: (blob) => {
        const window = this.doc.defaultView;
        if (window) {
          const url = window.URL.createObjectURL(blob);
          const link = this.doc.createElement('a');
          link.href = url;
          link.download = `attendance-${view}.${format}`;
          link.click();
          window.URL.revokeObjectURL(url);
        }
        this.exporting.set(false);
        this.statusMessage.set(`${format.toUpperCase()} export prepared for the current filters.`);
      },
      error: (error: unknown) => this.fail(error),
    });
  }

  private filter(): { cohortId?: string; from?: string; to?: string } {
    return {
      cohortId: this.cohortId() || undefined,
      from: this.from() ? `${this.from()}T00:00:00.000Z` : undefined,
      to: this.to() ? `${this.to()}T23:59:59.999Z` : undefined,
    };
  }

  private fail(error: unknown): void {
    this.loading.set(false);
    this.exporting.set(false);
    this.errorMessage.set(ApiError.from(error).title);
  }

  private daysAgo(days: number): string {
    const date = new Date();
    date.setDate(date.getDate() - days);
    return date.toISOString().slice(0, 10);
  }
}
