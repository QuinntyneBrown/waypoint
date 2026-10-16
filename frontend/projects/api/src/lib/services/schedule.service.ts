import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import { RsvpResult, SubmitRsvpInput, YouthSession } from '../models/engagement/schedule';

@Injectable({ providedIn: 'root' })
export class ScheduleService {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  mine(): Observable<YouthSession[]> {
    return this.http.get<YouthSession[]>(`${this.base}/api/me/schedule`);
  }

  submitRsvp(sessionId: string, input: SubmitRsvpInput): Observable<RsvpResult> {
    return this.http.post<RsvpResult>(
      `${this.base}/api/sessions/${encodeURIComponent(sessionId)}/rsvp`,
      input,
    );
  }
}
