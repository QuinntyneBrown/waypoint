import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import { SubmitApplicationRequest, SubmitApplicationResult } from '../models/intake/intake';

/** Anonymous application/referral intake (`POST /api/applications`). */
@Injectable({ providedIn: 'root' })
export class IntakeApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  submit(body: SubmitApplicationRequest): Observable<SubmitApplicationResult> {
    return this.http.post<SubmitApplicationResult>(`${this.base}/api/applications`, body);
  }
}
