import { DatePipe } from '@angular/common';
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
  StaffOperationsService,
  StaffSessionAttendance,
} from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  WButton,
  WFormField,
  WLabel,
  WRole,
  WShell,
  WTextField,
  WAlert,
  WCard,
  WCardBody,
  WBadge,
} from '@waypoint/components';
import {
  EditorDraftService,
  UnsavedChangesAware,
  DraftSaveResult,
  UnsavedChangesService,
} from '@waypoint/core';
import { AttendanceQueueService } from '@waypoint/core/attendance';

// Traces to: L2-ENGAGE-002/003, L2-NFR-004/006/007/009/010
@Component({
  selector: 'w-mentor-attendance-capture',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    WShell,
    DatePipe,
    RouterLink,
    WButton,
    WFormField,
    WLabel,
    WTextField,
    WAlert,
    WCard,
    WCardBody,
    WBadge,
  ],
  templateUrl: './mentor-attendance-capture.html',
  styles: `
    .attendance-summary {
      display: grid;
      grid-template-columns: repeat(5, minmax(0, 1fr));
      gap: var(--w-s3);
    }
    .attendance-summary > div {
      padding: var(--w-s3);
      text-align: center;
    }
    .attendance-summary strong {
      display: block;
      font-size: 1.25rem;
    }
    .attendance-row {
      display: grid;
      grid-template-columns: minmax(10rem, 0.5fr) minmax(22rem, 1fr) minmax(12rem, 0.6fr);
      gap: var(--w-s4);
      align-items: center;
      padding: var(--w-s4) 0;
      border-bottom: 1px solid var(--w-border);
    }
    .attendance-row:last-child {
      border-bottom: 0;
    }
    .attendance-actions {
      display: flex;
      flex-wrap: wrap;
      gap: var(--w-s2);
    }
    .attendance-actions .w-btn {
      min-width: 44px;
      min-height: 44px;
      flex: 1 1 auto;
    }
    .attendance-actions .is-selected {
      outline: 3px solid var(--w-ink);
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
      .attendance-actions .w-btn {
        flex: 1 1 calc(50% - var(--w-s2));
      }
    }
  `,
})
export class MentorAttendanceCapturePage implements UnsavedChangesAware {
  protected readonly auth = inject(AuthStore);
  protected readonly queue = inject(AttendanceQueueService);
  private readonly api = inject(StaffOperationsService);
  private readonly route = inject(ActivatedRoute);
  private readonly drafts = inject(EditorDraftService);
  private readonly unsavedChanges = inject(UnsavedChangesService);

  protected readonly sessionId = this.route.snapshot.paramMap.get('sessionId') ?? '';
  private readonly draftKey = `staff-attendance.${this.sessionId}`;
  protected readonly statuses: AttendanceStatus[] = ['Present', 'Late', 'Excused', 'Absent'];
  protected readonly session = signal<StaffSessionAttendance | undefined>(undefined);
  protected readonly rows = signal<StaffAttendanceRosterEntry[]>([]);
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  private readonly baseline = signal('');
  protected readonly isAdmin = computed(() => this.auth.user()?.role === 'ProgramAdministrator');
  protected readonly shellRole = computed<WRole>(() => (this.isAdmin() ? 'admin' : 'mentor'));
  protected readonly calendarPath = computed(() =>
    this.isAdmin() ? '/admin/calendar' : '/mentor/calendar',
  );
  protected readonly historyPath = computed(() =>
    this.isAdmin() ? '/admin/attendance' : '/mentor/attendance',
  );

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
          this.drafts.clear(this.draftKey);
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

  canDeactivate(): DraftSaveResult {
    return this.unsavedChanges.confirmSaveOrStay(this.dirty(), 'attendance', () =>
      this.saveLocalDraft(true),
    );
  }

  @HostListener('window:beforeunload', ['$event'])
  protected beforeUnload(event: BeforeUnloadEvent): void {
    this.unsavedChanges.warnBeforeUnload(event, this.dirty(), () => this.saveLocalDraft(false));
  }

  private loadRoster(resetBaseline = true): void {
    this.api.attendanceRoster(this.sessionId).subscribe({
      next: (session) => {
        this.session.set(session);
        const localDraft = resetBaseline
          ? this.drafts.take<StaffAttendanceRosterEntry[]>(this.draftKey)
          : null;
        const rows = localDraft
          ? session.roster.map((row) => {
              const saved = localDraft.find((item) => item.youthUserId === row.youthUserId);
              return saved
                ? { ...row, mark: saved.mark, note: saved.note, version: saved.version }
                : row;
            })
          : session.roster;
        this.rows.set(rows);
        if (resetBaseline || !this.baseline()) this.baseline.set(this.snapshot(session.roster));
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  private count(status: AttendanceStatus): number {
    return this.rows().filter((row) => row.mark === status).length;
  }

  private saveLocalDraft(markPristine: boolean): boolean {
    const rows = this.rows();
    const saved = this.drafts.save(this.draftKey, rows);
    if (saved && markPristine) this.baseline.set(this.snapshot(rows));
    return saved;
  }

  private snapshot(rows: StaffAttendanceRosterEntry[]): string {
    return JSON.stringify(rows.map((row) => [row.youthUserId, row.mark, row.note?.trim() ?? '']));
  }
}
