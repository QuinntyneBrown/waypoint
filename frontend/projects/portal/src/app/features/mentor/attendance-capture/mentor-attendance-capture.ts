import { DOCUMENT, DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  HostListener,
  inject,
  signal,
} from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  ApiError,
  AttendanceStatus,
  StaffAttendanceRosterEntry,
  StaffOperationsApi,
  StaffSessionAttendance,
} from '@wu/api';
import { AuthStore } from '@wu/auth';
import { WuShell } from '@wu/ui';
import { AttendanceQueueService } from '../../volunteer/attendance/attendance-queue.service';

// Traces to: L2-ENGAGE-002/003, L2-NFR-004/006/007/009/010
@Component({
  selector: 'wu-mentor-attendance-capture',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, DatePipe, RouterLink],
  templateUrl: './mentor-attendance-capture.html',
  styles: `
    .attendance-summary {
      display: grid;
      grid-template-columns: repeat(5, minmax(0, 1fr));
      gap: var(--wu-s3);
    }
    .attendance-summary > div {
      padding: var(--wu-s3);
      text-align: center;
    }
    .attendance-summary strong {
      display: block;
      font-size: var(--wu-text-xl);
    }
    .attendance-row {
      display: grid;
      grid-template-columns: minmax(10rem, 0.5fr) minmax(22rem, 1fr) minmax(12rem, 0.6fr);
      gap: var(--wu-s4);
      align-items: center;
      padding: var(--wu-s4) 0;
      border-bottom: 1px solid var(--wu-color-border);
    }
    .attendance-row:last-child {
      border-bottom: 0;
    }
    .attendance-actions {
      display: flex;
      flex-wrap: wrap;
      gap: var(--wu-s2);
    }
    .attendance-actions .wu-btn {
      min-width: 44px;
      min-height: 44px;
      flex: 1 1 auto;
    }
    .attendance-actions .is-selected {
      outline: 3px solid var(--wu-color-ink);
      outline-offset: 1px;
    }
    @media (max-width: 64rem) {
      .attendance-row {
        grid-template-columns: 1fr;
      }
    }
    @media (max-width: 42rem) {
      .attendance-summary {
        grid-template-columns: repeat(2, minmax(0, 1fr));
      }
      .attendance-summary > div:first-child {
        grid-column: 1 / -1;
      }
      .attendance-actions .wu-btn {
        flex: 1 1 calc(50% - var(--wu-s2));
      }
    }
  `,
})
export class MentorAttendanceCapturePage {
  protected readonly auth = inject(AuthStore);
  protected readonly queue = inject(AttendanceQueueService);
  private readonly api = inject(StaffOperationsApi);
  private readonly route = inject(ActivatedRoute);
  private readonly doc = inject(DOCUMENT);

  protected readonly sessionId = this.route.snapshot.paramMap.get('sessionId') ?? '';
  protected readonly statuses: AttendanceStatus[] = ['Present', 'Late', 'Excused', 'Absent'];
  protected readonly session = signal<StaffSessionAttendance | undefined>(undefined);
  protected readonly rows = signal<StaffAttendanceRosterEntry[]>([]);
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  private readonly baseline = signal('');

  protected readonly total = computed(() => this.rows().length);
  protected readonly marked = computed(() => this.rows().filter((row) => row.mark !== null).length);
  protected readonly present = computed(() => this.count('Present'));
  protected readonly late = computed(() => this.count('Late'));
  protected readonly excused = computed(() => this.count('Excused'));
  protected readonly absent = computed(() => this.count('Absent'));
  protected readonly dirty = computed(
    () => this.session() !== undefined && this.snapshot(this.rows()) !== this.baseline(),
  );

  constructor() {
    this.loadRoster();
  }

  protected mark(youthUserId: string, mark: AttendanceStatus | null): void {
    this.rows.update((rows) =>
      rows.map((row) => (row.youthUserId === youthUserId ? { ...row, mark } : row)),
    );
    this.errorMessage.set(null);
  }

  protected note(youthUserId: string, note: string): void {
    this.rows.update((rows) =>
      rows.map((row) => (row.youthUserId === youthUserId ? { ...row, note } : row)),
    );
  }

  protected markAll(mark: AttendanceStatus | null): void {
    this.rows.update((rows) => rows.map((row) => ({ ...row, mark })));
    this.errorMessage.set(null);
  }

  protected save(): void {
    const rows = this.rows();
    if (rows.length === 0 || this.marked() !== rows.length || this.saving()) return;
    this.saving.set(true);
    this.errorMessage.set(null);
    this.queue
      .save({
        sessionId: this.sessionId,
        marks: rows.map((row) => ({
          youthId: row.youthUserId,
          status: row.mark!,
          note: row.note?.trim() || null,
          expectedVersion: row.version,
        })),
      })
      .subscribe({
        next: (outcome) => {
          this.baseline.set(this.snapshot(rows));
          this.saving.set(false);
          if (outcome.kind === 'saved') this.loadRoster(false);
        },
        error: (error: unknown) => {
          const apiError = ApiError.from(error);
          this.saving.set(false);
          this.errorMessage.set(
            apiError.status === 409
              ? 'Attendance changed on the server. Your selections remain here; review and reconcile them.'
              : apiError.title,
          );
        },
      });
  }

  protected applyQueuedConflict(): void {
    this.api.attendanceRoster(this.sessionId).subscribe({
      next: (latest) => this.queue.retryConflicts(this.sessionId, latest.roster),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  protected keepServerAttendance(): void {
    this.queue.discardConflicts(this.sessionId);
    this.loadRoster();
  }

  canDeactivate(): boolean {
    if (!this.dirty()) return true;
    return (
      this.doc.defaultView?.confirm(
        'You have unsaved attendance changes. Leave this page and discard them?',
      ) ?? false
    );
  }

  @HostListener('window:beforeunload', ['$event'])
  protected beforeUnload(event: BeforeUnloadEvent): void {
    if (!this.dirty()) return;
    event.preventDefault();
    event.returnValue = '';
  }

  private loadRoster(resetBaseline = true): void {
    this.api.attendanceRoster(this.sessionId).subscribe({
      next: (session) => {
        this.session.set(session);
        this.rows.set(session.roster);
        if (resetBaseline || !this.baseline()) this.baseline.set(this.snapshot(session.roster));
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  private count(status: AttendanceStatus): number {
    return this.rows().filter((row) => row.mark === status).length;
  }

  private snapshot(rows: StaffAttendanceRosterEntry[]): string {
    return JSON.stringify(rows.map((row) => [row.youthUserId, row.mark, row.note?.trim() ?? '']));
  }
}
