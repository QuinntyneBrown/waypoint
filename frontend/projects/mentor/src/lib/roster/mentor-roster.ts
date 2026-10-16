import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { ApiError, MentorService, MentorRosterYouth } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WIcon, WShell } from '@waypoint/components';
import { debounceTime, switchMap, tap } from 'rxjs';

type RosterFilter = 'all' | 'on-track' | 'at-risk' | 'new';

// Traces to: L2-MENTOR-001, L2-AUTH-010
@Component({
  selector: 'w-mentor-roster',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, WIcon, RouterLink, DatePipe],
  templateUrl: './mentor-roster.html',
  styles: `
    .roster-controls {
      display: flex;
      align-items: flex-end;
      justify-content: space-between;
      flex-wrap: wrap;
      gap: var(--w-s4);
    }
    .roster-search {
      flex: 1 1 16rem;
      max-width: 24rem;
    }
    .filter-button {
      min-height: 44px;
    }
    progress {
      width: 8rem;
      max-width: 100%;
      accent-color: var(--w-lime-600);
    }
  `,
})
export class MentorRosterPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(MentorService);

  protected readonly search = signal('');
  protected readonly filter = signal<RosterFilter>('all');
  protected readonly youth = signal<MentorRosterYouth[]>([]);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  private readonly query = computed(() => ({ search: this.search(), filter: this.filter() }));

  constructor() {
    toObservable(this.query)
      .pipe(
        debounceTime(150),
        tap(() => {
          this.loading.set(true);
          this.errorMessage.set(null);
        }),
        switchMap(({ search, filter }) => this.api.roster(filter, search)),
        takeUntilDestroyed(),
      )
      .subscribe({
        next: (youth) => {
          this.youth.set(youth);
          this.loading.set(false);
        },
        error: (error: unknown) => {
          this.errorMessage.set(ApiError.from(error).title);
          this.loading.set(false);
        },
      });
  }

  protected onSearch(value: string): void {
    this.search.set(value);
  }

  protected setFilter(filter: RosterFilter): void {
    this.filter.set(filter);
  }

  protected clearFilters(): void {
    this.search.set('');
    this.filter.set('all');
  }

  protected percent(value: number): number {
    return Math.round(value * 100);
  }
}
