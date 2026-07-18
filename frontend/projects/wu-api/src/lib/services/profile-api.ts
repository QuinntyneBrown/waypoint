import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import { ChangePasswordRequest, UpdateProfileRequest } from '../models/platform/platform';

/** Current-user profile + credentials (`/api/me/*`, any authenticated user). */
@Injectable({ providedIn: 'root' })
export class ProfileApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  updateProfile(body: UpdateProfileRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/api/me/profile`, body);
  }

  changePassword(body: ChangePasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/api/me/change-password`, body);
  }
}
