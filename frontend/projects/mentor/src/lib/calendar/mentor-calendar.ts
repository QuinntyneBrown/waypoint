import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  ApiError,
  CalendarSession,
  SessionCohortOption,
  SessionSchedulingOptions,
  StaffOperationsService,
} from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WRole, WShell } from '@waypoint/components';

type CalendarView = 'month' | 'week' | 'agenda';

// Traces to: L2-ENGAGE-001, L2-AUTH-010, L2-NFR-009/010
@Component({
  selector: 'w-mentor-calendar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, DatePipe, RouterLink],
  templateUrl: './mentor-calendar.html',
  styles: `
    .calendar-layout {
      display: grid;
      grid-template-columns: minmax(0, 1fr) minmax(18rem, 0.34fr);
      gap: var(--w-s6);
      align-items: start;
    }
    .calendar-toolbar {
      display: flex;
      flex-wrap: wrap;
      align-items: end;
      justify-content: space-between;
      gap: var(--w-s3);
    }
    .calendar-scroll {
      max-width: 100%;
      overflow-x: auto;
    }
    .month-grid,
    .week-grid {
      display: grid;
      grid-template-columns: repeat(7, minmax(6.5rem, 1fr));
      min-width: 48rem;
      border: 1px solid var(--w-border);
      border-radius: var(--w-r-lg);
      overflow: hidden;
    }
    .calendar-weekday {
      padding: var(--w-s2);
      background: var(--w-g100);
      border-bottom: 1px solid var(--w-border);
      font-weight: 700;
      text-align: center;
    }
    .calendar-day {
      min-height: 8rem;
      padding: var(--w-s2);
      border-right: 1px solid var(--w-border);
      border-bottom: 1px solid var(--w-border);
    }
    .calendar-day:nth-child(7n) {
      border-right: 0;
    }
    .calendar-day.is-outside {
      color: var(--w-muted);
      background: var(--w-surface);
    }
    .calendar-event {
      display: block;
      margin-top: var(--w-s2);
      padding: var(--w-s2);
      border-radius: var(--w-r-card);
      background: var(--w-info-bg);
      color: inherit;
      text-decoration: none;
      overflow-wrap: anywhere;
    }
    .calendar-event:hover,
    .calendar-event:focus-visible {
      background: var(--w-surface-2);
    }
    .agenda-list {
      display: grid;
      gap: var(--w-s3);
    }
    .session-form {
      position: sticky;
      top: var(--w-s4);
    }
    .calendar-toolbar .w-btn,
    .calendar-event,
    [role='tab'] {
      min-height: 44px;
    }
    @media (max-width: 62rem) {
      .calendar-layout {
        grid-template-columns: 1fr;
      }
      .session-form {
        position: static;
      }
    }
    @media (max-width: 36rem) {
      .calendar-toolbar > *,
      .session-form .w-btn {
        width: 100%;
      }
    }
  `,
})
export class MentorCalendarPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(StaffOperationsService);

  protected readonly options = signal<SessionSchedulingOptions | null>(null);
  protected readonly sessions = signal<CalendarSession[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly statusMessage = signal<string | null>(null);
  protected readonly view = signal<CalendarView>('month');
  protected readonly anchor = signal(this.startOfDay(new Date()));
  protected readonly cohortFilter = signal('');
  protected readonly isAdmin = computed(() => this.auth.user()?.role === 'ProgramAdministrator');
  protected readonly shellRole = computed<WRole>(() => (this.isAdmin() ? 'admin' : 'mentor'));
  protected readonly attendanceBase = computed(() =>
    this.isAdmin() ? '/admin/attendance' : '/mentor/attendance',
  );

  protected readonly title = signal('');
  protected readonly cohortId = signal('');
  protected readonly mentorId = signal('');
  protected readonly date = signal(this.dateKey(this.addDays(new Date(), 1)));
  protected readonly time = signal('18:00');
  protected readonly durationMinutes = signal('90');
  protected readonly location = signal('');
  protected readonly repeatsWeekly = signal(false);

  protected readonly filteredSessions = computed(() =>
    this.sessions().filter(
      (session) => !this.cohortFilter() || session.cohortId === this.cohortFilter(),
    ),
  );
  protected readonly monthDays = computed(() => {
    const first = new Date(this.anchor().getFullYear(), this.anchor().getMonth(), 1);
    const start = this.addDays(first, -first.getDay());
    return Array.from({ length: 42 }, (_, index) => this.addDays(start, index));
  });
  protected readonly weekDays = computed(() => {
    const day = this.anchor();
    const mondayOffset = day.getDay() === 0 ? -6 : 1 - day.getDay();
    const start = this.addDays(day, mondayOffset);
    return Array.from({ length: 7 }, (_, index) => this.addDays(start, index));
  });
  protected readonly rangeLabel = computed(() => {
    const anchor = this.anchor();
    if (this.view() === 'month') {
      return anchor.toLocaleDateString(undefined, { month: 'long', year: 'numeric' });
    }
    if (this.view() === 'week') {
      const days = this.weekDays();
      return `${days[0].toLocaleDateString(undefined, { month: 'short', day: 'numeric' })}–${days[6].toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })}`;
    }
    return `Agenda from ${anchor.toLocaleDateString(undefined, { month: 'long', day: 'numeric', year: 'numeric' })}`;
  });
  protected readonly selectedCohort = computed<SessionCohortOption | undefined>(() =>
    this.options()?.cohorts.find((cohort) => cohort.cohortId === this.cohortId()),
  );

  constructor() {
    this.api.sessionOptions().subscribe({
      next: (options) => {
        this.options.set(options);
        this.cohortId.set(options.cohorts[0]?.cohortId ?? '');
        this.mentorId.set(options.mentors[0]?.mentorId ?? '');
        this.loadSessions();
      },
      error: (error: unknown) => this.fail(error),
    });
  }

  protected setView(view: CalendarView): void {
    this.view.set(view);
  }

  protected shift(direction: -1 | 1): void {
    const anchor = this.anchor();
    if (this.view() === 'month') {
      this.anchor.set(new Date(anchor.getFullYear(), anchor.getMonth() + direction, 1));
    } else {
      this.anchor.set(this.addDays(anchor, direction * (this.view() === 'week' ? 7 : 30)));
    }
    this.loadSessions();
  }

  protected today(): void {
    this.anchor.set(this.startOfDay(new Date()));
    this.loadSessions();
  }

  protected isCurrentMonth(day: Date): boolean {
    return day.getMonth() === this.anchor().getMonth();
  }

  protected sessionsFor(day: Date): CalendarSession[] {
    const key = this.dateKey(day);
    return this.filteredSessions().filter(
      (session) => this.dateKey(new Date(session.startsAt)) === key,
    );
  }

  protected agendaSessions(): CalendarSession[] {
    const from = this.anchor().getTime();
    return this.filteredSessions()
      .filter((session) => new Date(session.startsAt).getTime() >= from)
      .slice(0, 40);
  }

  protected createSession(): void {
    this.errorMessage.set(null);
    if (
      !this.title().trim() ||
      !this.cohortId() ||
      !this.mentorId() ||
      !this.date() ||
      !this.time()
    ) {
      this.errorMessage.set('Complete the title, cohort, mentor, date, and time.');
      return;
    }
    const startsAt = new Date(`${this.date()}T${this.time()}:00`);
    if (Number.isNaN(startsAt.getTime())) {
      this.errorMessage.set('Enter a valid date and time.');
      return;
    }
    this.saving.set(true);
    this.api
      .createSession({
        cohortId: this.cohortId(),
        mentorId: this.mentorId(),
        title: this.title().trim(),
        startsAt: startsAt.toISOString(),
        durationMinutes: Number(this.durationMinutes()) || null,
        location: this.location().trim() || null,
        repeatsWeekly: this.repeatsWeekly(),
      })
      .subscribe({
        next: (result) => {
          this.saving.set(false);
          this.statusMessage.set(
            result.generatedCount === 1
              ? 'Session scheduled.'
              : `${result.generatedCount} weekly sessions scheduled through the cohort term.`,
          );
          this.title.set('');
          this.anchor.set(this.startOfDay(startsAt));
          this.loadSessions();
        },
        error: (error: unknown) => this.fail(error),
      });
  }

  private loadSessions(): void {
    this.loading.set(true);
    const from = this.addDays(this.anchor(), -45).toISOString();
    const to = this.addDays(this.anchor(), 90).toISOString();
    this.api.sessions({ from, to }).subscribe({
      next: (sessions) => {
        this.sessions.set(sessions);
        this.loading.set(false);
      },
      error: (error: unknown) => this.fail(error),
    });
  }

  private fail(error: unknown): void {
    this.loading.set(false);
    this.saving.set(false);
    this.errorMessage.set(ApiError.from(error).title);
  }

  private startOfDay(date: Date): Date {
    return new Date(date.getFullYear(), date.getMonth(), date.getDate());
  }

  private addDays(date: Date, days: number): Date {
    const result = new Date(date);
    result.setDate(result.getDate() + days);
    return result;
  }

  private dateKey(date: Date): string {
    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
