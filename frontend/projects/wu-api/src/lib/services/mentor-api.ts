import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import {
  AtRiskYouth,
  FlagResolutionReason,
  MentorDashboardView,
  MentorReviewPreview,
  MentorRosterYouth,
  MentorYouthDetail,
  GradeSubmissionRequest,
  LogCheckInRequest,
  MentorCheckIn,
  MentorCompetencySummary,
  QuizAssessmentReview,
  SetCompetencyLevelRequest,
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

  quizAssessment(id: string): Observable<QuizAssessmentReview> {
    return this.http.get<QuizAssessmentReview>(
      `${this.base}/api/mentor/attempts/${encodeURIComponent(id)}`,
    );
  }

  approveQuizAttempt(id: string, overrideScore: number | null): Observable<void> {
    return this.http.post<void>(
      `${this.base}/api/mentor/attempts/${encodeURIComponent(id)}/approve`,
      overrideScore == null ? {} : { overrideScore },
    );
  }

  approveAllQuizAttempts(): Observable<number> {
    return this.http.post<number>(`${this.base}/api/mentor/attempts/approve-all`, {});
  }

  competencies(youthId: string): Observable<MentorCompetencySummary> {
    return this.http.get<MentorCompetencySummary>(
      `${this.base}/api/mentor/youth/${encodeURIComponent(youthId)}/competencies`,
    );
  }

  setCompetencyLevel(youthId: string, request: SetCompetencyLevelRequest): Observable<void> {
    return this.http.post<void>(
      `${this.base}/api/mentor/youth/${encodeURIComponent(youthId)}/competencies`,
      request,
    );
  }

  checkIns(youthId = '', type = ''): Observable<MentorCheckIn[]> {
    let params = new HttpParams();
    if (youthId) params = params.set('youthId', youthId);
    if (type) params = params.set('type', type);
    return this.http.get<MentorCheckIn[]>(`${this.base}/api/mentor/check-ins`, { params });
  }

  logCheckIn(request: LogCheckInRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.base}/api/mentor/check-ins`, request);
  }

  flags(): Observable<AtRiskYouth[]> {
    return this.http.get<AtRiskYouth[]>(`${this.base}/api/mentor/flags`);
  }

  raiseFlag(youthId: string, reason: string): Observable<void> {
    return this.http.post<void>(
      `${this.base}/api/mentor/youth/${encodeURIComponent(youthId)}/flag`,
      { reason },
    );
  }

  resolveFlag(flagId: string, resolutionReason: FlagResolutionReason): Observable<void> {
    return this.http.post<void>(`${this.base}/api/flags/${encodeURIComponent(flagId)}/resolve`, {
      resolutionReason,
    });
  }
}
