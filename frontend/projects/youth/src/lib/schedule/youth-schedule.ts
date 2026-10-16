import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ApiError, RsvpResponse, ScheduleService, YouthSession } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WShell } from '@waypoint/components';

interface RsvpDraft {
  response: RsvpResponse | null;
  note: string;
  reminderSet: boolean;
}

// Traces to: L2-ENGAGE-005, L2-ENGAGE-008
@Component({
  selector: 'w-youth-schedule',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, DatePipe],
  templateUrl: './youth-schedule.html',
  styles: `
    .schedule-list {
      display: grid;
      gap: var(--w-sp-5);
    }
    .session-card__layout {
      display: grid;
      grid-template-columns: minmax(12rem, 0.65fr) minmax(0, 1fr);
      gap: var(--w-sp-6);
      align-items: start;
    }
    .session-meta {
      margin: 0;
      padding: 0;
      list-style: none;
    }
    .rsvp-options {
      display: grid;
      grid-template-columns: repeat(3, minmax(0, 1fr));
      gap: var(--w-sp-2);
    }
    .rsvp-option {
      min-width: 0;
      margin: 0;
      padding: var(--w-sp-2);
      border: 1px solid var(--w-border);
      border-radius: var(--w-r-input);
    }
    .rsvp-note {
      min-height: 7rem;
    }
    @media (max-width: 48rem) {
      .session-card__layout {
        grid-template-columns: minmax(0, 1fr);
      }
      .rsvp-options {
        grid-template-columns: minmax(0, 1fr);
      }
    }
  `,
})
export class YouthSchedulePage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(ScheduleService);

  protected readonly sessions = signal<YouthSession[] | undefined>(undefined);
  protected readonly drafts = signal<Record<string, RsvpDraft>>({});
  protected readonly saving = signal<ReadonlySet<string>>(new Set());
  protected readonly statuses = signal<Record<string, string>>({});
  protected readonly errorMessage = signal<string | null>(null);

  constructor() {
    this.load();
  }

  protected draft(sessionId: string): RsvpDraft {
    return this.drafts()[sessionId] ?? { response: null, note: '', reminderSet: false };
  }

  protected setResponse(sessionId: string, response: RsvpResponse): void {
    this.updateDraft(sessionId, {
      response,
      reminderSet: response === "can't make it" ? false : this.draft(sessionId).reminderSet,
    });
  }

  protected setNote(sessionId: string, note: string): void {
    this.updateDraft(sessionId, { note });
  }

  protected setReminder(sessionId: string, reminderSet: boolean): void {
    this.updateDraft(sessionId, { reminderSet });
  }

  protected save(sessionId: string): void {
    const draft = this.draft(sessionId);
    if (!draft.response || this.saving().has(sessionId)) {
      this.statuses.update((current) => ({ ...current, [sessionId]: 'Choose an RSVP response.' }));
      return;
    }

    this.saving.update((current) => new Set(current).add(sessionId));
    this.statuses.update((current) => ({ ...current, [sessionId]: 'Saving RSVP…' }));
    this.api
      .submitRsvp(sessionId, {
        response: draft.response,
        note: draft.note.trim() || null,
        reminderSet: draft.response === "can't make it" ? false : draft.reminderSet,
      })
      .subscribe({
        next: () => {
          this.saving.update((current) => {
            const next = new Set(current);
            next.delete(sessionId);
            return next;
          });
          this.statuses.update((current) => ({ ...current, [sessionId]: 'RSVP saved.' }));
          this.load(false);
        },
        error: (error: unknown) => {
          this.saving.update((current) => {
            const next = new Set(current);
            next.delete(sessionId);
            return next;
          });
          this.statuses.update((current) => ({
            ...current,
            [sessionId]: ApiError.from(error).title,
          }));
        },
      });
  }

  private updateDraft(sessionId: string, changes: Partial<RsvpDraft>): void {
    this.drafts.update((current) => ({
      ...current,
      [sessionId]: { ...this.draft(sessionId), ...changes },
    }));
  }

  private load(showLoading = true): void {
    if (showLoading) this.sessions.set(undefined);
    this.errorMessage.set(null);
    this.api.mine().subscribe({
      next: (sessions) => {
        this.sessions.set(sessions);
        this.drafts.set(
          Object.fromEntries(
            sessions.map((session) => [
              session.sessionId,
              {
                response: this.toInputResponse(session.response),
                note: session.note ?? '',
                reminderSet: session.reminderSet,
              },
            ]),
          ),
        );
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  private toInputResponse(response: YouthSession['response']): RsvpResponse | null {
    if (response === 'Going') return 'going';
    if (response === 'Maybe') return 'maybe';
    if (response === 'CantAttend') return "can't make it";
    return null;
  }
}
