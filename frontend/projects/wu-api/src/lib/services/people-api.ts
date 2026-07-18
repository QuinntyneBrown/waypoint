import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import { DirectoryCohort, ListPeopleParams, PagedPeople } from '../models/people/people';

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
}
