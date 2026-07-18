import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import { YouthCurriculum } from '../models/learning/curriculum';

@Injectable({ providedIn: 'root' })
export class LearningApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  curriculum(trackId?: string): Observable<YouthCurriculum> {
    const suffix = trackId ? `/${encodeURIComponent(trackId)}` : '';
    return this.http.get<YouthCurriculum>(`${this.base}/api/me/curriculum${suffix}`);
  }
}
