import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import {
  RosterImportRequest,
  RosterImportResult,
  RosterPreview,
} from '../models/reporting/roster-import';

@Injectable({ providedIn: 'root' })
export class DataTransferService {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  previewRoster(request: RosterImportRequest): Observable<RosterPreview> {
    return this.http.post<RosterPreview>(`${this.base}/api/admin/import/preview`, request);
  }

  importRoster(request: RosterImportRequest): Observable<RosterImportResult> {
    return this.http.post<RosterImportResult>(`${this.base}/api/admin/import`, request);
  }
}
