import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import {
  CohortDetail,
  CohortOptions,
  CohortResult,
  CohortSummary,
  CreateCohortRequest,
  EditCohortRequest,
  ProgressBoard,
} from '../models/cohorts/cohorts';

@Injectable({ providedIn: 'root' })
export class CohortsApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  list(): Observable<CohortSummary[]> {
    return this.http.get<CohortSummary[]>(`${this.base}/api/cohorts`);
  }

  options(): Observable<CohortOptions> {
    return this.http.get<CohortOptions>(`${this.base}/api/cohorts/options`);
  }

  detail(id: string): Observable<CohortDetail> {
    return this.http.get<CohortDetail>(`${this.base}/api/cohorts/${id}`);
  }

  create(request: CreateCohortRequest): Observable<CohortResult> {
    return this.http.post<CohortResult>(`${this.base}/api/cohorts`, request);
  }

  edit(id: string, request: EditCohortRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/api/cohorts/${id}/edit`, request);
  }

  addYouth(id: string, youthId: string): Observable<void> {
    return this.http.post<void>(`${this.base}/api/cohorts/${id}/youth`, { youthId });
  }

  progress(cohortId: string, trackId: string): Observable<ProgressBoard> {
    return this.http.get<ProgressBoard>(`${this.base}/api/cohorts/${cohortId}/progress`, {
      params: { trackId },
    });
  }

  exportProgress(cohortId: string, trackId: string, format: 'csv' | 'pdf'): Observable<Blob> {
    return this.http.get(`${this.base}/api/cohorts/${cohortId}/progress/export`, {
      params: { trackId, format },
      responseType: 'blob',
    });
  }
}
