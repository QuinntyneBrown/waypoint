import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import { NotificationPreference } from '../models/preferences/notification-preferences';

/** Per-user notification preferences (`/api/me/notification-preferences`). */
@Injectable({ providedIn: 'root' })
export class NotificationPreferencesApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  /** Returns only persisted rows; a fresh user has none (frontend applies defaults). */
  list(): Observable<NotificationPreference[]> {
    return this.http.get<NotificationPreference[]>(`${this.base}/api/me/notification-preferences`);
  }

  save(pref: NotificationPreference): Observable<void> {
    return this.http.put<void>(`${this.base}/api/me/notification-preferences`, pref);
  }
}
