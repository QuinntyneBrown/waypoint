import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import {
  DirectoryCohort,
  ListPeopleParams,
  PagedPeople,
  PersonDetail,
} from '../models/people/people';

/** Program-administration people directory (`/api/people`, admin only). */
@Injectable({ providedIn: 'root' })
export class PeopleApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  /** `GET /api/people` — server-side paged/filtered directory (L2-ADMIN-001). */
  list(opts: ListPeopleParams = {}): Observable<PagedPeople> {
    let params = new HttpParams();
    if (opts.page != null) params = params.set('page', opts.page);
    if (opts.pageSize != null) params = params.set('pageSize', opts.pageSize);
    if (opts.role) params = params.set('role', opts.role);
    if (opts.cohortId) params = params.set('cohortId', opts.cohortId);
    if (opts.search) params = params.set('search', opts.search);
    return this.http.get<PagedPeople>(`${this.base}/api/people`, { params });
  }

  cohorts(): Observable<DirectoryCohort[]> {
    return this.http.get<DirectoryCohort[]>(`${this.base}/api/cohorts`);
  }

  detail(id: string): Observable<PersonDetail> {
    return this.http.get<PersonDetail>(`${this.base}/api/people/${id}`);
  }

  changeRole(id: string, role: string): Observable<void> {
    return this.http.post<void>(`${this.base}/api/people/${id}/role`, { role });
  }

  sendPasswordReset(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/api/people/${id}/send-password-reset`, {});
  }

  deactivate(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/api/people/${id}/deactivate`, {});
  }

  reactivate(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/api/people/${id}/reactivate`, {});
  }

  remove(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/api/people/${id}/remove`, {});
  }
}
