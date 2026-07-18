import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ApiError, CohortOptions, CohortsApi, CohortSummary, ProgressBoard } from '@wu/api';
import { AuthStore } from '@wu/auth';
import { WuIcon, WuShell } from '@wu/ui';
import { debounceTime, filter, finalize, forkJoin, merge } from 'rxjs';
import { RealtimeService } from '../../../realtime.service';

// Traces to: L2-ADMIN-004, L2-REPORT-003, L2-NFR-005
@Component({
  selector: 'wu-progress-board',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, WuIcon],
  templateUrl: './progress-board.html',
  styles: `
    .board-filters {
      display: grid;
      grid-template-columns: repeat(2, minmax(13rem, 1fr));
      gap: var(--wu-sp-4);
      align-items: end;
    }
    .board-actions {
      display: flex;
      flex-wrap: wrap;
      gap: var(--wu-sp-3);
    }
    .matrix-wrap {
      max-width: 100%;
      overflow-x: auto;
      overscroll-behavior-inline: contain;
      border: 1px solid var(--wu-border);
      border-radius: var(--wu-radius-lg);
    }
    .matrix {
      width: 100%;
      border-collapse: collapse;
    }
    .matrix th,
    .matrix td {
      padding: var(--wu-sp-3);
      border-bottom: 1px solid var(--wu-border);
      text-align: left;
      vertical-align: middle;
    }
    .matrix thead th {
      position: sticky;
      top: 0;
      background: var(--wu-surface);
      z-index: 1;
    }
    .matrix th:first-child,
    .matrix td:first-child {
      position: sticky;
      left: 0;
      min-width: 13rem;
      background: var(--wu-surface);
      z-index: 2;
    }
    .matrix thead th:first-child {
      z-index: 3;
    }
    .module-cell {
      min-width: 9rem;
    }
    .module-cell__name {
      display: block;
      margin-bottom: var(--wu-sp-1);
      color: var(--wu-text-muted);
      font-size: var(--wu-text-sm);
    }
    .state {
      display: inline-flex;
      align-items: center;
      min-height: 2rem;
      padding: 0 var(--wu-sp-2);
      border-radius: var(--wu-radius-pill);
      background: var(--wu-surface-subtle);
      font-weight: 700;
      white-space: nowrap;
    }
    .state--done {
      color: var(--wu-success);
    }
    .state--progress {
      color: var(--wu-info);
    }
    .state--unassigned {
      color: var(--wu-text-muted);
      font-weight: 500;
    }
    .matrix tfoot th,
    .matrix tfoot td {
      border-bottom: 0;
      background: var(--wu-surface-subtle);
      font-weight: 700;
    }
    @media (max-width: 42rem) {
      .board-filters {
        grid-template-columns: 1fr;
      }
      .board-actions .wu-btn {
        flex: 1 1 calc(50% - var(--wu-sp-3));
        min-height: 45px;
      }
    }
  `,
})
export class ProgressBoardPage {
  protected readonly auth = inject(AuthStore);
  protected readonly realtime = inject(RealtimeService);
  private readonly api = inject(CohortsApi);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly cohorts = signal<CohortSummary[]>([]);
  protected readonly options = signal<CohortOptions>({ tracks: [], mentors: [] });
  protected readonly cohortId = signal('');
  protected readonly trackId = signal('');
  protected readonly board = signal<ProgressBoard | undefined>(undefined);
  protected readonly loadingOptions = signal(true);
  protected readonly exporting = signal<'csv' | 'pdf' | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly matrixWidth = computed(() => 390 + (this.board()?.modules.length ?? 0) * 160);

  constructor() {
    this.loadOptions();
    merge(
      this.realtime.progressChanged$.pipe(filter((event) => event.cohortId === this.cohortId())),
      this.realtime.reconnected$,
    )
      .pipe(debounceTime(100), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.loadBoard(false));
  }

  protected selectCohort(id: string): void {
    this.cohortId.set(id);
    const cohort = this.cohorts().find((item) => item.id === id);
    this.trackId.set(cohort?.trackId ?? this.options().tracks[0]?.id ?? '');
    this.loadBoard();
  }

  protected selectTrack(id: string): void {
    this.trackId.set(id);
    this.loadBoard();
  }

  protected refresh(): void {
    this.loadBoard();
  }

  protected stateLabel(state: string): string {
    switch (state) {
      case 'InProgress':
        return 'In progress';
      case 'NotStarted':
        return 'Not started';
      case 'NotAssigned':
        return 'Not assigned';
      default:
        return state;
    }
  }

  protected percent(value: number): number {
    return Math.round(value * 100);
  }

  protected whole(value: number): number {
    return Math.round(value);
  }

  protected download(format: 'csv' | 'pdf'): void {
    const cohortId = this.cohortId();
    const trackId = this.trackId();
    if (!cohortId || !trackId || this.exporting()) return;
    this.exporting.set(format);
    this.errorMessage.set(null);
    this.api
      .exportProgress(cohortId, trackId, format)
      .pipe(finalize(() => this.exporting.set(null)))
      .subscribe({
        next: (blob) => {
          const url = URL.createObjectURL(blob);
          const anchor = document.createElement('a');
          anchor.href = url;
          anchor.download = `progress-${cohortId}-${trackId}.${format}`;
          anchor.click();
          URL.revokeObjectURL(url);
        },
        error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
      });
  }

  private loadOptions(): void {
    this.loadingOptions.set(true);
    this.errorMessage.set(null);
    forkJoin({ cohorts: this.api.list(), options: this.api.options() }).subscribe({
      next: ({ cohorts, options }) => {
        this.cohorts.set(cohorts);
        this.options.set(options);
        const cohort = cohorts[0];
        this.cohortId.set(cohort?.id ?? '');
        this.trackId.set(cohort?.trackId ?? options.tracks[0]?.id ?? '');
        this.loadingOptions.set(false);
        this.loadBoard();
      },
      error: (error: unknown) => {
        this.loadingOptions.set(false);
        this.errorMessage.set(ApiError.from(error).title);
      },
    });
  }

  private loadBoard(showLoading = true): void {
    const cohortId = this.cohortId();
    const trackId = this.trackId();
    if (!cohortId || !trackId) {
      this.board.set(undefined);
      return;
    }
    if (showLoading) this.board.set(undefined);
    this.errorMessage.set(null);
    this.api.progress(cohortId, trackId).subscribe({
      next: (board) => this.board.set(board),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }
}
