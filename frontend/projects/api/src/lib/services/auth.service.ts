import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import { AuthResult, RefreshRequest, RegisterRequest, SignInRequest } from '../models/auth/auth';

/**
 * Auth endpoints (`/api/auth/*`). Tokens are returned in the JSON body only.
 * The register endpoint is not surfaced in the UI (invite-only provisioning);
 * it exists here for the e2e data channel and future flows.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  register(body: RegisterRequest): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${this.base}/api/auth/register`, body);
  }

  signIn(body: SignInRequest): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${this.base}/api/auth/sign-in`, body);
  }

  refresh(body: RefreshRequest): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${this.base}/api/auth/refresh`, body);
  }

  signOut(): Observable<void> {
    return this.http.post<void>(`${this.base}/api/auth/sign-out`, {});
  }
}
