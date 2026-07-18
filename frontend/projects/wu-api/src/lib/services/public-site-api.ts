import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import {
  PublicContentView,
  SubmitContactRequest,
  SubmitContactResult,
  SubmitDonationPledgeRequest,
  SubmitDonationPledgeResult,
  SubmitVolunteerSignupRequest,
  SubmitVolunteerSignupResult,
} from '../models/public/public';

/**
 * Anonymous public marketing-site form submissions (contact, volunteer sign-up,
 * donation pledge). Every endpoint is `AllowAnonymous` on the server.
 */
@Injectable({ providedIn: 'root' })
export class PublicSiteApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  /** `GET /api/public/content` (L2-PUB-002). */
  getContent(): Observable<PublicContentView> {
    return this.http.get<PublicContentView>(`${this.base}/api/public/content`);
  }

  /** `POST /api/public/contact` (L2-PUB-003 AC1). */
  submitContact(body: SubmitContactRequest): Observable<SubmitContactResult> {
    return this.http.post<SubmitContactResult>(`${this.base}/api/public/contact`, body);
  }

  /** `POST /api/public/volunteer-signup` (L2-PUB-003 AC2). */
  submitVolunteerSignup(
    body: SubmitVolunteerSignupRequest,
  ): Observable<SubmitVolunteerSignupResult> {
    return this.http.post<SubmitVolunteerSignupResult>(
      `${this.base}/api/public/volunteer-signup`,
      body,
    );
  }

  /** `POST /api/public/donation` (L2-PUB-003 AC3). */
  submitDonation(body: SubmitDonationPledgeRequest): Observable<SubmitDonationPledgeResult> {
    return this.http.post<SubmitDonationPledgeResult>(`${this.base}/api/public/donation`, body);
  }
}
