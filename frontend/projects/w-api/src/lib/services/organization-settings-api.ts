import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import {
  OrganizationSettings,
  ProgramTerm,
  ProgramTrackSetting,
  SaveProgramTerm,
  TermResult,
} from '../models/settings/organization-settings';

@Injectable({ providedIn: 'root' })
export class OrganizationSettingsApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  get(): Observable<OrganizationSettings> {
    return this.http.get<OrganizationSettings>(`${this.base}/api/org-settings`);
  }

  update(settings: OrganizationSettings): Observable<void> {
    return this.http.put<void>(`${this.base}/api/org-settings`, settings);
  }

  terms(): Observable<ProgramTerm[]> {
    return this.http.get<ProgramTerm[]>(`${this.base}/api/org-settings/terms`);
  }

  createTerm(term: SaveProgramTerm): Observable<TermResult> {
    return this.http.post<TermResult>(`${this.base}/api/org-settings/terms`, term);
  }

  updateTerm(id: string, term: SaveProgramTerm): Observable<void> {
    return this.http.put<void>(`${this.base}/api/org-settings/terms/${id}`, term);
  }

  tracks(): Observable<ProgramTrackSetting[]> {
    return this.http.get<ProgramTrackSetting[]>(`${this.base}/api/org-settings/tracks`);
  }

  setTrackEnabled(id: string, enabled: boolean): Observable<void> {
    return this.http.post<void>(`${this.base}/api/org-settings/tracks/${id}/enabled`, {
      enabled,
    });
  }
}
