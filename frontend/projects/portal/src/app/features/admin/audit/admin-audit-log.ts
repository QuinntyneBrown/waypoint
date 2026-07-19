import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ApiError, AuditApi, AuditLogFilter, AuditLogPage } from '@wu/api';
import { AuthStore } from '@wu/auth';
import { WuShell } from '@wu/ui';
import { finalize } from 'rxjs/operators';

// Traces to: L2-CONSENT-003, L2-REPORT-003, L2-AUTH-009, L2-NFR-009/010
@Component({
  selector: 'wu-admin-audit-log',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, ReactiveFormsModule, DatePipe],
  templateUrl: './admin-audit-log.html',
  styles: `
    .audit-filters {
      display: grid;
      grid-template-columns: repeat(4, minmax(9rem, 1fr)) auto auto;
      gap: var(--wu-sp-3);
      align-items: end;
    }
    .audit-target,
    .audit-source,
    .audit-detail {
      overflow-wrap: anywhere;
    }
    .audit-private {
      display: inline-flex;
      gap: var(--wu-sp-2);
      align-items: center;
      color: var(--wu-muted);
      font-weight: 700;
    }
    .audit-pagination {
      display: flex;
      flex-wrap: wrap;
      gap: var(--wu-sp-3);
      align-items: center;
      justify-content: space-between;
    }
    .audit-pagination__actions {
      display: flex;
      gap: var(--wu-sp-2);
    }
    @media (max-width: 68rem) {
      .audit-filters {
        grid-template-columns: repeat(2, minmax(0, 1fr));
      }
    }
    @media (max-width: 48rem) {
      .audit-filters {
        grid-template-columns: 1fr;
      }
      .audit-filters .wu-btn,
      .audit-pagination .wu-btn,
      .wu-page__actions .wu-btn {
        width: 100%;
        min-height: 45px;
      }
      .audit-pagination,
      .audit-pagination__actions {
        align-items: stretch;
        flex-direction: column;
      }
    }
  `,
})
export class AdminAuditLogPage {
  private static readonly pageSize = 10;
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(AuditApi);
  private readonly fb = inject(FormBuilder);

  protected readonly result = signal<AuditLogPage | undefined>(undefined);
  protected readonly loading = signal(true);
  protected readonly exporting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly dateError = signal<string | null>(null);
  protected readonly exportStatus = signal<string | null>(null);
  protected readonly pageCount = computed(() =>
    Math.max(1, Math.ceil((this.result()?.total ?? 0) / AdminAuditLogPage.pageSize)),
  );
  protected readonly firstShown = computed(() => {
    const result = this.result();
    return result && result.total > 0 ? (result.page - 1) * result.pageSize + 1 : 0;
  });
  protected readonly lastShown = computed(() => {
    const result = this.result();
    return result ? Math.min(result.page * result.pageSize, result.total) : 0;
  });
  protected readonly filterForm = this.fb.nonNullable.group({
    actorUserId: [''],
    action: [''],
    from: [''],
    to: [''],
  });

  constructor() {
    this.load(1);
  }

  protected applyFilters(): void {
    const value = this.filterForm.getRawValue();
    if (value.from && value.to && value.to < value.from) {
      this.dateError.set('Date to must be on or after date from.');
      return;
    }
    this.dateError.set(null);
    this.load(1);
  }

  protected clearFilters(): void {
    this.filterForm.reset({ actorUserId: '', action: '', from: '', to: '' });
    this.dateError.set(null);
    this.load(1);
  }

  protected changePage(page: number): void {
    if (page < 1 || page > this.pageCount() || this.loading()) return;
    this.load(page);
  }

  protected retry(): void {
    this.load(this.result()?.page ?? 1);
  }

  protected exportCsv(): void {
    if (this.exporting()) return;
    this.exporting.set(true);
    this.exportStatus.set(null);
    this.errorMessage.set(null);
    this.api
      .exportCsv(this.filter())
      .pipe(finalize(() => this.exporting.set(false)))
      .subscribe({
        next: (blob) => {
          const url = URL.createObjectURL(blob);
          const anchor = document.createElement('a');
          anchor.href = url;
          anchor.download = `audit-log-${Date.now()}.csv`;
          anchor.click();
          URL.revokeObjectURL(url);
          this.exportStatus.set('CSV export prepared for the current audit filters.');
        },
        error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
      });
  }

  protected actionLabel(action: string): string {
    return action
      .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
      .replace(/[.:_-]+/g, ' ')
      .replace(/^./, (letter) => letter.toUpperCase());
  }

  protected targetLabel(targetType?: string, targetId?: string): string {
    if (!targetType && !targetId) return '—';
    return `${targetType ?? 'Record'}${targetId ? ` · ${targetId.slice(0, 8)}` : ''}`;
  }

  private filter(page?: number): AuditLogFilter {
    const value = this.filterForm.getRawValue();
    return {
      page,
      pageSize: page === undefined ? undefined : AdminAuditLogPage.pageSize,
      actorUserId: value.actorUserId || undefined,
      action: value.action || undefined,
      from: value.from || undefined,
      to: value.to || undefined,
    };
  }

  private load(page: number): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.exportStatus.set(null);
    this.api
      .list(this.filter(page))
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (result) => this.result.set(result),
        error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
      });
  }
}
