import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import { ForgotPasswordRequest, ResetPasswordRequest } from '../models/auth/password-reset';

/** Password-reset endpoints (`/api/auth/password-reset/*`), both anonymous. */
@Injectable({ providedIn: 'root' })
export class PasswordResetApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  /** Always resolves 204 whether or not the email exists (no enumeration). */
  requestReset(body: ForgotPasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/api/auth/password-reset/request`, body);
  }

  /** 204 on success; 410 when the link is invalid/expired; 400 on policy/mismatch. */
  confirmReset(body: ResetPasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/api/auth/password-reset/confirm`, body);
  }
}
