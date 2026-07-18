import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import { VolunteerTodayView } from '../models/volunteer/volunteer';

/**
 * Volunteer portal queries (`/api/volunteer`). Strictly volunteer-scoped: no
 * youth records are ever returned (L2-VOLUNTEER-001).
 */
@Injectable({ providedIn: 'root' })
export class VolunteerApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  /** `GET /api/volunteer/today` — null (204) when there is no session today. */
  today(): Observable<VolunteerTodayView | null> {
    return this.http.get<VolunteerTodayView | null>(`${this.base}/api/volunteer/today`);
  }
}
