import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  ApiError,
  CohortOptions,
  CohortsService,
  CohortSummary,
  ProgressBoard,
} from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  WButton,
  WFormField,
  WIcon,
  WLabel,
  WSelect,
  WShell,
  WAlert,
  WCard,
  WCardBody,
  WBadge,
} from '@waypoint/components';
import { debounceTime, filter, finalize, forkJoin, merge } from 'rxjs';
import { RealtimeService } from '@waypoint/core/realtime';

// Traces to: L2-ADMIN-004, L2-REPORT-003, L2-NFR-005
@Component({
  selector: 'w-progress-board',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, WIcon, WButton, WFormField, WLabel, WSelect, WAlert, WCard, WCardBody, WBadge],
  templateUrl: './progress-board.html',
  styles: `
    .board-filters {
      display: grid;
      grid-template-columns: repeat(2, minmax(13rem, 1fr));
      gap: var(--w-s4);
      align-items: end;
    }
    .board-actions {
      display: flex;
      flex-wrap: wrap;
      gap: var(--w-s3);
    }
    @media (max-width: 42rem) {
      .board-filters {
        grid-template-columns: 1fr;
      }
      .board-actions .w-btn {
        flex: 1 1 calc(50% - var(--w-s3));
        min-height: 45px;
      }
    }
  `,
})
export class ProgressBoardPage {
  protected readonly auth = inject(AuthStore);
  protected readonly realtime = inject(RealtimeService);
  private readonly api = inject(CohortsService);
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
