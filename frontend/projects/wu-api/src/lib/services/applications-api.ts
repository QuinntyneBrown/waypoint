import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import { ApplicationSummary } from '../models/applications/applications';

/** Admin application review (`/api/applications`, admin only for reads). */
@Injectable({ providedIn: 'root' })
export class ApplicationsApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  /** `GET /api/applications` — the review queue, optionally filtered by status. */
  list(status?: string | null): Observable<ApplicationSummary[]> {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    return this.http.get<ApplicationSummary[]>(`${this.base}/api/applications`, { params });
  }
}
