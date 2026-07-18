import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import {
  GuardianAnnouncement,
  GuardianChildProgress,
  GuardianChildSummary,
  GuardianConsentRecord,
  GuardianContact,
  GuardianContactUpdate,
  GuardianMediaConsentUpdate,
} from '../models/guardian/guardian';

@Injectable({ providedIn: 'root' })
export class GuardianApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  children(): Observable<GuardianChildSummary[]> {
    return this.http.get<GuardianChildSummary[]>(`${this.base}/api/guardian/children`);
  }

  progress(childId: string): Observable<GuardianChildProgress> {
    return this.http.get<GuardianChildProgress>(
      `${this.base}/api/guardian/children/${childId}/progress`,
    );
  }

  announcements(): Observable<GuardianAnnouncement[]> {
    return this.http.get<GuardianAnnouncement[]>(`${this.base}/api/guardian/announcements`);
  }

  markAllAnnouncementsRead(): Observable<void> {
    return this.http.post<void>(`${this.base}/api/guardian/announcements/read-all`, {});
  }

  contact(): Observable<GuardianContact> {
    return this.http.get<GuardianContact>(`${this.base}/api/guardian/contact`);
  }

  updateContact(update: GuardianContactUpdate): Observable<void> {
    return this.http.put<void>(`${this.base}/api/guardian/contact`, update);
  }

  consent(childId: string): Observable<GuardianConsentRecord[]> {
    return this.http.get<GuardianConsentRecord[]>(
      `${this.base}/api/guardian/children/${childId}/consent`,
    );
  }

  updateMediaConsent(childId: string, update: GuardianMediaConsentUpdate): Observable<void> {
    return this.http.put<void>(
      `${this.base}/api/guardian/children/${childId}/media-consent`,
      update,
    );
  }

  emailConsentCopy(consentId: string): Observable<void> {
    return this.http.post<void>(`${this.base}/api/guardian/consents/${consentId}/email-copy`, {});
  }

  withdrawConsent(consentId: string): Observable<void> {
    return this.http.post<void>(`${this.base}/api/consent/${consentId}/withdraw`, {});
  }
}
