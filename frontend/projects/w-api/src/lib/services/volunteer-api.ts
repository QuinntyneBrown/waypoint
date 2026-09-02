import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import {
  VolunteerRosterEntry,
  VolunteerScheduleItem,
  VolunteerTodayView,
} from '../models/volunteer/volunteer';
import { AttendanceTally, AttendanceWrite } from '../models/engagement/operations';

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

  /** `GET /api/volunteer/schedule` — upcoming assigned sessions (empty when none). */
  schedule(): Observable<VolunteerScheduleItem[]> {
    return this.http.get<VolunteerScheduleItem[]>(`${this.base}/api/volunteer/schedule`);
  }

  /** Names-only roster for a session assigned to the signed-in volunteer. */
  roster(sessionId: string): Observable<VolunteerRosterEntry[]> {
    return this.http.get<VolunteerRosterEntry[]>(
      `${this.base}/api/volunteer/sessions/${sessionId}/roster`,
    );
  }

  /** Save or replay one attendance operation with a durable idempotency key. */
  saveAttendance(write: AttendanceWrite, idempotencyKey: string): Observable<AttendanceTally> {
    return this.http.post<AttendanceTally>(`${this.base}/api/attendance`, write, {
      headers: { 'Idempotency-Key': idempotencyKey },
    });
  }
}
