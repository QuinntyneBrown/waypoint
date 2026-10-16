import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import {
  CohortReport,
  ReportFilter,
  ReportExportFormat,
  ReportExportKind,
  ReportOptions,
  ReportsHub,
  StaffActivityReportRow,
  YouthReport,
} from '../models/reporting/reports';

@Injectable({ providedIn: 'root' })
export class ReportsService {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  options(): Observable<ReportOptions> {
    return this.http.get<ReportOptions>(`${this.base}/api/reports/options`);
  }

  hub(filter: ReportFilter = {}): Observable<ReportsHub> {
    return this.http.get<ReportsHub>(`${this.base}/api/reports/hub`, {
      params: this.params(filter),
    });
  }

  youth(id: string, filter: ReportFilter = {}): Observable<YouthReport> {
    return this.http.get<YouthReport>(`${this.base}/api/reports/youth/${encodeURIComponent(id)}`, {
      params: this.params({ from: filter.from, to: filter.to, trackId: filter.trackId }),
    });
  }

  cohort(
    id: string,
    filter: ReportFilter = {},
    compareCohortId?: string,
  ): Observable<CohortReport> {
    let params = this.params({ from: filter.from, to: filter.to, trackId: filter.trackId });
    if (compareCohortId) params = params.set('compareCohortId', compareCohortId);
    return this.http.get<CohortReport>(
      `${this.base}/api/reports/cohort/${encodeURIComponent(id)}`,
      { params },
    );
  }

  staff(filter: ReportFilter = {}): Observable<StaffActivityReportRow[]> {
    return this.http.get<StaffActivityReportRow[]>(`${this.base}/api/reports/staff-activity`, {
      params: this.params(filter),
    });
  }

  exportReport(
    report: ReportExportKind,
    format: ReportExportFormat,
    filter: ReportFilter = {},
    youthId?: string,
    cohortId?: string,
    compareCohortId?: string,
  ): Observable<Blob> {
    let params = this.params(filter).set('report', report).set('format', format);
    if (youthId) params = params.set('youthId', youthId);
    if (cohortId) params = params.set('cohortId', cohortId);
    if (compareCohortId) params = params.set('compareCohortId', compareCohortId);
    return this.http.get(`${this.base}/api/reports/export`, { params, responseType: 'blob' });
  }

  private params(filter: ReportFilter): HttpParams {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filter)) {
      if (value) params = params.set(key, value);
    }
    return params;
  }
}
