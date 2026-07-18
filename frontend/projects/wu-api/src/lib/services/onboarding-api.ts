import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import { CompleteOnboardingRequest, OnboardingStatus } from '../models/onboarding/onboarding';

/** Youth onboarding endpoints (`/api/onboarding`, Youth-only). */
@Injectable({ providedIn: 'root' })
export class OnboardingApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  status(): Observable<OnboardingStatus> {
    return this.http.get<OnboardingStatus>(`${this.base}/api/onboarding`);
  }

  /** Marks onboarding completed. Cohort/mentor are read-only and not sent. */
  complete(body: CompleteOnboardingRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/api/onboarding`, body);
  }

  /** "Skip for now": records the skip; onboarding stays resumable at next sign-in. */
  skip(): Observable<void> {
    return this.http.post<void>(`${this.base}/api/onboarding/skip`, {});
  }
}
