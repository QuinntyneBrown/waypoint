import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import { DisplayPreferences } from '../models/preferences/display-preferences';

/** Per-user display & accessibility preferences (`/api/me/display-preferences`). */
@Injectable({ providedIn: 'root' })
export class DisplayPreferencesApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  get(): Observable<DisplayPreferences> {
    return this.http.get<DisplayPreferences>(`${this.base}/api/me/display-preferences`);
  }

  save(prefs: DisplayPreferences): Observable<DisplayPreferences> {
    return this.http.put<DisplayPreferences>(`${this.base}/api/me/display-preferences`, prefs);
  }
}
