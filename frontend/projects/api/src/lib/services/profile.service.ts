import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import {
  ChangePasswordRequest,
  MyProfile,
  ProfilePhoto,
  UpdateProfileRequest,
} from '../models/platform/platform';

/** Current-user profile + credentials (`/api/me/*`, any authenticated user). */
@Injectable({ providedIn: 'root' })
export class ProfileService {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  updateProfile(body: UpdateProfileRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/api/me/profile`, body);
  }

  getProfile(): Observable<MyProfile> {
    return this.http.get<MyProfile>(`${this.base}/api/me/profile`);
  }

  uploadPhoto(file: File): Observable<ProfilePhoto> {
    const form = new FormData();
    form.append('file', file, file.name);
    return this.http.post<ProfilePhoto>(`${this.base}/api/me/profile/photo`, form);
  }

  photo(path: string): Observable<Blob> {
    return this.http.get(`${this.base}${path}`, { responseType: 'blob' });
  }

  removePhoto(): Observable<void> {
    return this.http.delete<void>(`${this.base}/api/me/profile/photo`);
  }

  changePassword(body: ChangePasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/api/me/change-password`, body);
  }
}
