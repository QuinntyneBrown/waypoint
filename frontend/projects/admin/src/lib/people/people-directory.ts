import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { ApiError, DirectoryCohort, PagedPeople, PeopleApi } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WIcon, WShell } from '@waypoint/components';
import { catchError, debounceTime, of, switchMap, tap } from 'rxjs';

interface DirectoryQuery {
  search: string;
  role: string;
  cohortId: string;
  page: number;
}

// Traces to: L2-ADMIN-001
@Component({
  selector: 'w-people-directory',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, WIcon, RouterLink, DatePipe],
  templateUrl: './people-directory.html',
  styles: `
    .directory-filters {
      display: grid;
      grid-template-columns: minmax(14rem, 1fr) minmax(13rem, 0.5fr);
      gap: var(--w-sp-4);
      align-items: end;
    }
    .role-filters {
      grid-column: 1 / -1;
      margin: 0;
      padding: 0;
      border: 0;
    }
    .role-filter-list {
      display: flex;
      flex-wrap: wrap;
      gap: var(--w-sp-2);
    }
    .pager {
      margin-top: var(--w-sp-4);
    }
    @media (max-width: 42rem) {
      .directory-filters {
        grid-template-columns: 1fr;
      }
      .role-filters {
        grid-column: auto;
      }
      .role-filter-list .w-btn {
        flex: 1 1 calc(50% - var(--w-sp-2));
      }
    }
  `,
})
export class PeopleDirectory {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(PeopleApi);
  private static readonly pageSize = 10;

  protected readonly search = signal('');
  protected readonly role = signal('');
  protected readonly cohortId = signal('');
  protected readonly page = signal(1);
  protected readonly result = signal<PagedPeople | null>(null);
  protected readonly cohorts = signal<DirectoryCohort[]>([]);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly query = computed<DirectoryQuery>(() => ({
    search: this.search(),
    role: this.role(),
    cohortId: this.cohortId(),
    page: this.page(),
  }));
  protected readonly people = computed(() => this.result()?.items ?? []);
  protected readonly loading = computed(() => this.result() === null);
  protected readonly totalPages = computed(() =>
    Math.max(1, Math.ceil((this.result()?.total ?? 0) / PeopleDirectory.pageSize)),
  );
  protected readonly hasFilters = computed(
    () => !!this.search() || !!this.role() || !!this.cohortId(),
  );

  constructor() {
    this.api.cohorts().subscribe({
      next: (cohorts) => this.cohorts.set(cohorts),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
    toObservable(this.query)
      .pipe(
        debounceTime(150),
        tap(() => {
          this.result.set(null);
          this.errorMessage.set(null);
        }),
        switchMap((query) =>
          this.api
            .list({
              page: query.page,
              pageSize: PeopleDirectory.pageSize,
              search: query.search || null,
              role: query.role || null,
              cohortId: query.cohortId || null,
            })
            .pipe(
              catchError((error: unknown) => {
                this.errorMessage.set(ApiError.from(error).title);
                return of({
                  items: [],
                  total: 0,
                  page: query.page,
                  pageSize: PeopleDirectory.pageSize,
                });
              }),
            ),
        ),
        takeUntilDestroyed(),
      )
      .subscribe((result) => this.result.set(result));
  }

  protected onSearch(value: string): void {
    this.page.set(1);
    this.search.set(value);
  }

  protected setRole(role: string): void {
    this.page.set(1);
    this.role.set(role);
  }

  protected setCohort(cohortId: string): void {
    this.page.set(1);
    this.cohortId.set(cohortId);
  }

  protected clearFilters(): void {
    this.page.set(1);
    this.search.set('');
    this.role.set('');
    this.cohortId.set('');
  }

  protected previousPage(): void {
    this.page.update((page) => Math.max(1, page - 1));
  }

  protected nextPage(): void {
    this.page.update((page) => Math.min(this.totalPages(), page + 1));
  }

  protected cohortName(cohortId: string | null): string {
    return this.cohorts().find((cohort) => cohort.id === cohortId)?.name ?? '—';
  }
}
