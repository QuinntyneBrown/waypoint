import { DOCUMENT, DatePipe, PercentPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  ApiError,
  SessionAttendanceSummary,
  SessionSchedulingOptions,
  StaffOperationsService,
  YouthAttendanceSummary,
} from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  WButton,
  WFormField,
  WLabel,
  WRole,
  WSelect,
  WShell,
  WTextField,
  WAlert,
  WCard,
  WCardHeader,
  WCardTitle,
  WCardBody,
  WBadge,
} from '@waypoint/components';
import { forkJoin } from 'rxjs';

type HistoryView = 'session' | 'youth';

// Traces to: L2-ENGAGE-004, L2-REPORT-003, L2-NFR-009/010
@Component({
  selector: 'w-mentor-attendance-history',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    WShell,
    DatePipe,
    PercentPipe,
    RouterLink,
    WButton,
    WFormField,
    WLabel,
    WTextField,
    WSelect,
    WAlert,
    WCard,
    WCardHeader,
    WCardTitle,
    WCardBody,
    WBadge,
  ],
  templateUrl: './mentor-attendance-history.html',
  styles: `
    .history-toolbar {
      display: flex;
      flex-wrap: wrap;
      align-items: end;
      justify-content: space-between;
      gap: var(--w-s4);
    }
    .history-filters {
      display: flex;
      flex-wrap: wrap;
      align-items: end;
      gap: var(--w-s3);
    }
    [role='tab'],
    .history-toolbar .w-btn,
    .w-table .w-btn {
      min-height: 44px;
    }
    @media (max-width: 42rem) {
      .history-filters,
      .history-filters .w-field,
      .history-filters .w-btn,
      .history-toolbar > .w-cluster {
        width: 100%;
      }
      .history-toolbar > .w-cluster .w-btn {
        flex: 1;
      }
    }
  `,
})
export class MentorAttendanceHistoryPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(StaffOperationsService);
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
  private loadSequence = 0;
  protected readonly isAdmin = computed(() => this.auth.user()?.role === 'ProgramAdministrator');
  protected readonly shellRole = computed<WRole>(() => (this.isAdmin() ? 'admin' : 'mentor'));
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
    const sequence = ++this.loadSequence;
    this.loading.set(true);
    this.errorMessage.set(null);
    const filter = this.filter();
    forkJoin({
      sessions: this.api.sessionHistory(filter),
      youth: this.api.youthHistory(filter),
    }).subscribe({
      next: ({ sessions, youth }) => {
        if (sequence !== this.loadSequence) return;
        this.sessions.set(sessions);
        this.youth.set(youth);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        if (sequence === this.loadSequence) this.fail(error);
      },
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
