import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import {
  ConsentDelivery,
  ConsentReceipt,
  RequestedConsent,
  RequestedConsentCapture,
  ConsentStatus,
} from '../models/consent/consent';

/** Youth consent status (`/api/consent/status`, Youth-only). Server-authoritative gate. */
@Injectable({ providedIn: 'root' })
export class ConsentApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  status(): Observable<ConsentStatus> {
    return this.http.get<ConsentStatus>(`${this.base}/api/consent/status`);
  }

  requestGuardian(): Observable<ConsentDelivery> {
    return this.http.post<ConsentDelivery>(`${this.base}/api/consent/request`, {});
  }

  requested(token: string): Observable<RequestedConsent> {
    return this.http.get<RequestedConsent>(
      `${this.base}/api/public/guardian-consent/${encodeURIComponent(token)}`,
    );
  }

  captureRequested(token: string, capture: RequestedConsentCapture): Observable<ConsentReceipt> {
    return this.http.post<ConsentReceipt>(
      `${this.base}/api/public/guardian-consent/${encodeURIComponent(token)}`,
      capture,
    );
  }
}
