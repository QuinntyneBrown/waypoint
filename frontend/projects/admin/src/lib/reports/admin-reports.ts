import { DatePipe, DecimalPipe, PercentPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import {
  ApiError,
  CohortReport,
  ReportFilter,
  ReportOptions,
  ReportsService,
  ReportsHub,
  YouthReport,
} from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  Button,
  ErrorText,
  FormField,
  Icon,
  Label,
  Select,
  Shell,
  TextField,
  Alert,
  AlertTitle,
  Card,
  CardBody,
  Badge,
  EmptyState,
  EmptyStateIcon,
  EmptyStateTitle,
  EmptyStateText,
  Progress,
  Stat,
  StatLabel,
  StatValue,
  ProgressRing,
  ProgressRingLabel,
  Tabs,
  Tab,
} from '@waypoint/components';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';

type ReportView = 'overview' | 'youth' | 'cohort' | 'staff';

// Traces to: L2-REPORT-001, L2-AUTH-010, L2-NFR-009/010
@Component({
  selector: 'ph-admin-reports',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    Shell,
    Icon,
    ReactiveFormsModule,
    DatePipe,
    DecimalPipe,
    PercentPipe,
    Button,
    FormField,
    Label,
    ErrorText,
    TextField,
    Select,
    Alert,
    AlertTitle,
    Card,
    CardBody,
    Badge,
    EmptyState,
    EmptyStateIcon,
    EmptyStateTitle,
    EmptyStateText,
    Progress,
    Stat,
    StatLabel,
    StatValue,
    ProgressRing,
    ProgressRingLabel,
    Tabs,
    Tab,
  ],
  templateUrl: './admin-reports.html',
  styleUrl: './admin-reports.css',
})
export class AdminReportsPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(ReportsService);
  private readonly fb = inject(FormBuilder);

  protected readonly view = signal<ReportView>('overview');
  protected readonly options = signal<ReportOptions | undefined>(undefined);
  protected readonly hub = signal<ReportsHub | undefined>(undefined);
  protected readonly youthReport = signal<YouthReport | undefined>(undefined);
  protected readonly cohortReport = signal<CohortReport | undefined>(undefined);
  protected readonly loadingDetail = signal(false);
  protected readonly exporting = signal<'csv' | 'pdf' | null>(null);
  protected readonly exportStatus = signal<string | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly dateError = signal<string | null>(null);
  protected readonly filterForm = this.fb.nonNullable.group({
    from: [''],
    to: [''],
    cohortId: [''],
    trackId: [''],
    mentorId: [''],
  });
  protected readonly youthForm = this.fb.nonNullable.group({ youthId: [''] });
  protected readonly cohortForm = this.fb.nonNullable.group({
    cohortId: [''],
    compareCohortId: [''],
  });

  constructor() {
    this.load();
  }

  protected selectView(view: ReportView): void {
    this.view.set(view);
    this.exportStatus.set(null);
  }

  protected applyFilters(): void {
    const value = this.filterForm.getRawValue();
    if (value.from && value.to && value.to < value.from) {
      this.dateError.set('Date to must be on or after date from.');
      return;
    }
    this.dateError.set(null);
    this.youthReport.set(undefined);
    this.cohortReport.set(undefined);
    this.loadHub(this.filter(), true);
  }

  protected generateYouth(): void {
    const youthId = this.youthForm.controls.youthId.value;
    if (!youthId || this.loadingDetail()) return;
    this.loadingDetail.set(true);
    this.errorMessage.set(null);
    this.api.youth(youthId, this.filter()).subscribe({
      next: (report) => {
        this.youthReport.set(report);
        this.loadingDetail.set(false);
      },
      error: (error: unknown) => {
        this.errorMessage.set(ApiError.from(error).title);
        this.loadingDetail.set(false);
      },
    });
  }

  protected generateCohort(): void {
    const cohortId = this.cohortForm.controls.cohortId.value;
    if (!cohortId || this.loadingDetail()) return;
    this.loadingDetail.set(true);
    this.errorMessage.set(null);
    this.api
      .cohort(cohortId, this.filter(), this.cohortForm.controls.compareCohortId.value || undefined)
      .subscribe({
        next: (report) => {
          this.cohortReport.set(report);
          this.loadingDetail.set(false);
        },
        error: (error: unknown) => {
          this.errorMessage.set(ApiError.from(error).title);
          this.loadingDetail.set(false);
        },
      });
  }

  protected retry(): void {
    this.load();
  }

  protected canExport(): boolean {
    if (this.hub() === undefined) return false;
    if (this.view() === 'youth') {
      return this.youthReport()?.youthId === this.youthForm.controls.youthId.value;
    }
    if (this.view() === 'cohort') {
      return this.cohortReport()?.cohortId === this.cohortForm.controls.cohortId.value;
    }
    return true;
  }

  protected exportCurrent(format: 'csv' | 'pdf'): void {
    if (!this.canExport() || this.exporting()) return;
    const report = this.view();
    const youthId =
      report === 'youth' ? this.youthForm.controls.youthId.value || undefined : undefined;
    const cohortId =
      report === 'cohort' ? this.cohortForm.controls.cohortId.value || undefined : undefined;
    const compareCohortId =
      report === 'cohort' ? this.cohortForm.controls.compareCohortId.value || undefined : undefined;
    this.exporting.set(format);
    this.exportStatus.set(null);
    this.errorMessage.set(null);
    this.api
      .exportReport(report, format, this.filter(), youthId, cohortId, compareCohortId)
      .pipe(finalize(() => this.exporting.set(null)))
      .subscribe({
        next: (blob) => {
          const url = URL.createObjectURL(blob);
          const anchor = document.createElement('a');
          anchor.href = url;
          anchor.download = `${report}-report-${Date.now()}.${format}`;
          anchor.click();
          URL.revokeObjectURL(url);
          this.exportStatus.set(
            `${format.toUpperCase()} export prepared for the current report and filters.`,
          );
        },
        error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
      });
  }

  protected checkInLabel(count: number): string {
    return `${count} mentor check-in${count === 1 ? '' : 's'}`;
  }

  private filter(): ReportFilter {
    const value = this.filterForm.getRawValue();
    return {
      from: value.from || undefined,
      to: value.to || undefined,
      cohortId: value.cohortId || undefined,
      trackId: value.trackId || undefined,
      mentorId: value.mentorId || undefined,
    };
  }

  private load(): void {
    this.options.set(undefined);
    this.hub.set(undefined);
    this.errorMessage.set(null);
    forkJoin({ options: this.api.options(), hub: this.api.hub() }).subscribe({
      next: ({ options, hub }) => {
        this.options.set(options);
        this.hub.set(hub);
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  private loadHub(filter: ReportFilter, clear = false): void {
    if (clear) this.hub.set(undefined);
    this.errorMessage.set(null);
    this.api.hub(filter).subscribe({
      next: (hub) => this.hub.set(hub),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }
}
