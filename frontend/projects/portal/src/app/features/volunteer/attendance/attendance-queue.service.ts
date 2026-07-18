import { DOCUMENT } from '@angular/common';
import { computed, effect, inject, Injectable, signal } from '@angular/core';
import { ApiError, AttendanceTally, AttendanceWrite, VolunteerApi } from '@wu/api';
import { AuthStore } from '@wu/auth';
import { OfflineService } from '@wu/ui';
import { catchError, map, Observable, of, throwError } from 'rxjs';

interface QueuedAttendanceWrite {
  idempotencyKey: string;
  ownerUserId: string;
  createdAt: string;
  state: 'queued' | 'conflict';
  write: AttendanceWrite;
}

interface OwnerQueue {
  ownerUserId: string;
  entries: QueuedAttendanceWrite[];
}

export type AttendanceSaveOutcome = { kind: 'saved'; tally: AttendanceTally } | { kind: 'queued' };

// Traces to: L2-ENGAGE-003, L2-NFR-004
@Injectable({ providedIn: 'root' })
export class AttendanceQueueService {
  private readonly doc = inject(DOCUMENT);
  private readonly api = inject(VolunteerApi);
  private readonly auth = inject(AuthStore);
  private readonly offline = inject(OfflineService);

  private readonly _pendingCount = signal(0);
  private readonly _message = signal<string | null>(null);
  private syncing = false;

  readonly pendingCount = this._pendingCount.asReadonly();
  readonly message = this._message.asReadonly();
  readonly hasConflict = computed(() => {
    this._pendingCount();
    return this.currentEntries().some((entry) => entry.state === 'conflict');
  });

  constructor() {
    effect(() => {
      const ownerUserId = this.auth.user()?.userId ?? null;
      const isOffline = this.offline.isOffline();
      this.updatePending(ownerUserId);
      if (ownerUserId && !isOffline) {
        this.syncNext(ownerUserId);
      }
    });
  }

  save(write: AttendanceWrite): Observable<AttendanceSaveOutcome> {
    const ownerUserId = this.auth.user()?.userId;
    if (!ownerUserId) {
      return throwError(() => new Error('Attendance cannot be saved without a signed-in owner.'));
    }

    const entry: QueuedAttendanceWrite = {
      idempotencyKey: globalThis.crypto.randomUUID(),
      ownerUserId,
      createdAt: new Date().toISOString(),
      state: 'queued',
      write,
    };

    if (this.offline.isOffline()) {
      this.enqueue(entry);
      return of({ kind: 'queued' });
    }

    return this.api.saveAttendance(write, entry.idempotencyKey).pipe(
      map((tally) => {
        this._message.set('Attendance saved.');
        return { kind: 'saved', tally } as const;
      }),
      catchError((error: unknown) => {
        if (ApiError.from(error).status === 0) {
          this.enqueue(entry);
          return of({ kind: 'queued' } as const);
        }
        return throwError(() => error);
      }),
    );
  }

  hasConflictForSession(sessionId: string): boolean {
    this._pendingCount();
    return this.currentEntries().some(
      (entry) => entry.state === 'conflict' && entry.write.sessionId === sessionId,
    );
  }

  /** Rebase a rejected offline operation onto the versions the user has just reviewed. */
  retryConflicts(
    sessionId: string,
    latestRoster: ReadonlyArray<{ youthUserId: string; version: string | null }>,
  ): void {
    const ownerUserId = this.auth.user()?.userId;
    if (!ownerUserId) return;
    const versions = new Map(latestRoster.map((row) => [row.youthUserId, row.version]));
    const queue = this.read(ownerUserId);
    for (const entry of queue.entries) {
      if (entry.state !== 'conflict' || entry.write.sessionId !== sessionId) continue;
      entry.idempotencyKey = globalThis.crypto.randomUUID();
      entry.state = 'queued';
      entry.write = {
        ...entry.write,
        marks: entry.write.marks.map((mark) => ({
          ...mark,
          expectedVersion: versions.get(mark.youthId) ?? 'none',
        })),
      };
    }
    this.write(queue);
    this._message.set('Applying your reviewed attendance changes…');
    this.syncNext(ownerUserId);
  }

  discardConflicts(sessionId: string): void {
    const ownerUserId = this.auth.user()?.userId;
    if (!ownerUserId) return;
    const queue = this.read(ownerUserId);
    queue.entries = queue.entries.filter(
      (entry) => entry.state !== 'conflict' || entry.write.sessionId !== sessionId,
    );
    this.write(queue);
    this._message.set('Kept the newer server attendance and removed the offline conflict.');
  }

  private enqueue(entry: QueuedAttendanceWrite): void {
    const queue = this.read(entry.ownerUserId);
    queue.entries.push(entry);
    this.write(queue);
    this._message.set(
      `Saved offline — ${queue.entries.length} attendance update${queue.entries.length === 1 ? '' : 's'} waiting to sync.`,
    );
  }

  private syncNext(ownerUserId: string): void {
    if (this.syncing || this.offline.isOffline()) return;
    const queue = this.read(ownerUserId);
    const next = queue.entries.find((entry) => entry.state === 'queued');
    if (!next) return;

    this.syncing = true;
    this._message.set(
      `Syncing ${queue.entries.filter((entry) => entry.state === 'queued').length} attendance update${queue.entries.length === 1 ? '' : 's'}…`,
    );
    this.api.saveAttendance(next.write, next.idempotencyKey).subscribe({
      next: () => {
        const latest = this.read(ownerUserId);
        latest.entries = latest.entries.filter(
          (entry) => entry.idempotencyKey !== next.idempotencyKey,
        );
        this.write(latest);
        this.syncing = false;
        if (latest.entries.some((entry) => entry.state === 'queued')) {
          this.syncNext(ownerUserId);
        } else if (latest.entries.some((entry) => entry.state === 'conflict')) {
          this._message.set('A queued attendance change needs review before it can sync.');
        } else {
          this._message.set('All attendance changes synced.');
        }
      },
      error: (error: unknown) => {
        const apiError = ApiError.from(error);
        this.syncing = false;
        if (apiError.status === 409) {
          const latest = this.read(ownerUserId);
          const conflicted = latest.entries.find(
            (entry) => entry.idempotencyKey === next.idempotencyKey,
          );
          if (conflicted) conflicted.state = 'conflict';
          this.write(latest);
          this._message.set(
            'A queued attendance change conflicts with a newer server edit. Review it before saving again.',
          );
          this.syncNext(ownerUserId);
        }
      },
    });
  }

  private currentEntries(): QueuedAttendanceWrite[] {
    const ownerUserId = this.auth.user()?.userId;
    return ownerUserId ? this.read(ownerUserId).entries : [];
  }

  private updatePending(ownerUserId: string | null): void {
    this._pendingCount.set(ownerUserId ? this.read(ownerUserId).entries.length : 0);
  }

  private read(ownerUserId: string): OwnerQueue {
    const raw = this.storage?.getItem(this.storageKey(ownerUserId));
    if (!raw) return { ownerUserId, entries: [] };
    try {
      const parsed = JSON.parse(raw) as OwnerQueue;
      if (parsed.ownerUserId !== ownerUserId || !Array.isArray(parsed.entries)) {
        return { ownerUserId, entries: [] };
      }
      return {
        ownerUserId,
        entries: parsed.entries.filter((entry) => entry.ownerUserId === ownerUserId),
      };
    } catch {
      return { ownerUserId, entries: [] };
    }
  }

  private write(queue: OwnerQueue): void {
    if (!this.storage) return;
    if (queue.entries.length === 0) {
      this.storage.removeItem(this.storageKey(queue.ownerUserId));
    } else {
      this.storage.setItem(this.storageKey(queue.ownerUserId), JSON.stringify(queue));
    }
    this._pendingCount.set(queue.entries.length);
  }

  private storageKey(ownerUserId: string): string {
    return `wu.attendance.queue.${ownerUserId}`;
  }

  private get storage(): Storage | null {
    return this.doc.defaultView?.localStorage ?? null;
  }
}
