import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import {
  MentorDashboardView,
  MentorReviewPreview,
  MentorRosterYouth,
  MentorYouthDetail,
  GradeSubmissionRequest,
  SubmissionReviewView,
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

  youth(id: string): Observable<MentorYouthDetail> {
    return this.http.get<MentorYouthDetail>(
      `${this.base}/api/mentor/youth/${encodeURIComponent(id)}`,
    );
  }

  submission(id: string): Observable<SubmissionReviewView> {
    return this.http.get<SubmissionReviewView>(
      `${this.base}/api/mentor/submissions/${encodeURIComponent(id)}`,
    );
  }

  gradeSubmission(id: string, request: GradeSubmissionRequest): Observable<void> {
    return this.http.post<void>(
      `${this.base}/api/mentor/submissions/${encodeURIComponent(id)}/grade`,
      request,
    );
  }
}
