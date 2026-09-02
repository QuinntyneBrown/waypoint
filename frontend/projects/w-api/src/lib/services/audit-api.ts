import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import { AuditLogFilter, AuditLogPage } from '../models/consent/audit-log';

@Injectable({ providedIn: 'root' })
export class AuditApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  list(filter: AuditLogFilter = {}): Observable<AuditLogPage> {
    return this.http.get<AuditLogPage>(`${this.base}/api/audit`, {
      params: this.params(filter),
    });
  }

  exportCsv(filter: AuditLogFilter = {}): Observable<Blob> {
    return this.http.get(`${this.base}/api/admin/audit/export`, {
      params: this.params(filter).set('format', 'csv'),
      responseType: 'blob',
    });
  }

  private params(filter: AuditLogFilter): HttpParams {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filter)) {
      if (value !== undefined && value !== '') params = params.set(key, String(value));
    }
    return params;
  }
}
