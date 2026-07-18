import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { debounceTime, switchMap } from 'rxjs';
import { PagedPeople, PeopleApi } from '@wu/api';
import { AuthStore } from '@wu/auth';
import { WuShell } from '@wu/ui';

// Traces to: L2-ADMIN-001
@Component({
  selector: 'wu-people-directory',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, RouterLink, DatePipe],
  templateUrl: './people-directory.html',
})
export class PeopleDirectory {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(PeopleApi);

  protected readonly search = signal('');
  protected readonly result = signal<PagedPeople | null>(null);

  protected readonly people = computed(() => this.result()?.items ?? []);
  protected readonly loading = computed(() => this.result() === null);

  constructor() {
    // Re-query (server-side) whenever the search term settles; switchMap cancels
    // any in-flight request so results never arrive out of order.
    toObservable(this.search)
      .pipe(
        debounceTime(150),
        switchMap((term) => this.api.list({ search: term || null })),
        takeUntilDestroyed(),
      )
      .subscribe((result) => this.result.set(result));
  }

  protected onSearch(value: string): void {
    this.search.set(value);
  }
}
