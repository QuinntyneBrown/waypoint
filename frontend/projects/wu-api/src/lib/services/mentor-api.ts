import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import {
  MentorDashboardView,
  MentorReviewPreview,
  MentorRosterYouth,
} from '../models/mentoring/mentor';

@Injectable({ providedIn: 'root' })
export class MentorApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  dashboard(): Observable<MentorDashboardView> {
    return this.http.get<MentorDashboardView>(`${this.base}/api/mentor/dashboard`);
  }

  roster(filter = 'all', search = ''): Observable<MentorRosterYouth[]> {
    let params = new HttpParams().set('filter', filter);
    if (search.trim()) params = params.set('search', search.trim());
    return this.http.get<MentorRosterYouth[]>(`${this.base}/api/mentor/youth`, { params });
  }

  reviews(take = 5): Observable<MentorReviewPreview[]> {
    return this.http.get<MentorReviewPreview[]>(`${this.base}/api/mentor/reviews`, {
      params: new HttpParams().set('take', take),
    });
  }
}
