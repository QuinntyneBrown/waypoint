import { DOCUMENT } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  HostListener,
  inject,
  signal,
} from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiError, VolunteerApi, VolunteerRosterEntry } from '@wu/api';
import { AuthStore } from '@wu/auth';
import { WuShell } from '@wu/ui';
import { AttendanceQueueService } from './attendance-queue.service';

type VolunteerMark = 'Present' | 'Absent' | null;

interface AttendanceRow extends VolunteerRosterEntry {
  mark: VolunteerMark;
}

// Traces to: L2-VOLUNTEER-001, L2-ENGAGE-002/003, L2-NFR-004/007
@Component({
  selector: 'wu-volunteer-attendance',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, RouterLink],
  templateUrl: './volunteer-attendance.html',
  styles: [
    `
      .attendance-summary {
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: var(--wu-space-3);
      }
      .attendance-summary > div {
        padding: var(--wu-space-3);
        text-align: center;
      }
      .attendance-summary strong {
        display: block;
        font-size: var(--wu-text-xl);
      }
      .attendance-row {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: var(--wu-space-4);
        padding: var(--wu-space-4) 0;
        border-bottom: 1px solid var(--wu-color-border);
      }
      .attendance-row:last-child {
        border-bottom: 0;
      }
      .attendance-name {
        font-weight: 700;
      }
      .attendance-actions {
        display: flex;
        gap: var(--wu-space-2);
      }
      .attendance-actions .is-selected {
        outline: 3px solid var(--wu-color-ink);
        outline-offset: 2px;
      }
      @media (max-width: 575px) {
        .attendance-row {
          align-items: stretch;
          flex-direction: column;
        }
        .attendance-actions .wu-btn {
          flex: 1;
        }
      }
    `,
  ],
})
export class VolunteerAttendance {
  protected readonly auth = inject(AuthStore);
  protected readonly queue = inject(AttendanceQueueService);
  private readonly api = inject(VolunteerApi);
  private readonly route = inject(ActivatedRoute);
  private readonly doc = inject(DOCUMENT);

  protected readonly sessionId = this.route.snapshot.paramMap.get('sessionId') ?? '';
  private readonly baseline = signal('');
  protected readonly rows = signal<AttendanceRow[] | undefined>(undefined);
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly marked = computed(
    () => this.rows()?.filter((row) => row.mark !== null).length ?? 0,
  );
  protected readonly present = computed(
    () => this.rows()?.filter((row) => row.mark === 'Present').length ?? 0,
  );
  protected readonly absent = computed(
    () => this.rows()?.filter((row) => row.mark === 'Absent').length ?? 0,
  );
  protected readonly total = computed(() => this.rows()?.length ?? 0);
  protected readonly dirty = computed(() => {
    const rows = this.rows();
    return rows !== undefined && this.snapshot(rows) !== this.baseline();
  });

  constructor() {
    this.loadRoster();
  }

  protected mark(youthUserId: string, mark: VolunteerMark): void {
    this.rows.update((rows) =>
      rows?.map((row) => (row.youthUserId === youthUserId ? { ...row, mark } : row)),
    );
    this.errorMessage.set(null);
  }

  protected markAll(mark: VolunteerMark): void {
    this.rows.update((rows) => rows?.map((row) => ({ ...row, mark })));
    this.errorMessage.set(null);
  }

  protected save(): void {
    const rows = this.rows();
    if (!rows || this.marked() !== rows.length || this.saving()) return;

    this.saving.set(true);
    this.errorMessage.set(null);
    this.queue
      .save({
        sessionId: this.sessionId,
        marks: rows.map((row) => ({
          youthId: row.youthUserId,
          status: row.mark!,
          note: null,
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
              ? 'Attendance changed on the server. Your selections are still here; reload the roster and reconcile them.'
              : apiError.title,
          );
        },
      });
  }

  protected applyQueuedConflict(): void {
    this.api.roster(this.sessionId).subscribe({
      next: (latest) => this.queue.retryConflicts(this.sessionId, latest),
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
    this.api.roster(this.sessionId).subscribe({
      next: (rows) => {
        this.rows.set(rows);
        if (resetBaseline || !this.baseline()) this.baseline.set(this.snapshot(rows));
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  private snapshot(rows: AttendanceRow[]): string {
    return JSON.stringify(rows.map((row) => [row.youthUserId, row.mark]));
  }
}
