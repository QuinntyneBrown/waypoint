import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import { SubmitContactRequest, SubmitContactResult } from '../models/public/public';

/**
 * Anonymous public marketing-site form submissions (contact, volunteer sign-up,
 * donation pledge). Every endpoint is `AllowAnonymous` on the server.
 */
@Injectable({ providedIn: 'root' })
export class PublicSiteApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  /** `POST /api/public/contact` (L2-PUB-003 AC1). */
  submitContact(body: SubmitContactRequest): Observable<SubmitContactResult> {
    return this.http.post<SubmitContactResult>(`${this.base}/api/public/contact`, body);
  }
}
