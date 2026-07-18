import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import {
  AnnouncementView,
  NotificationView,
  SupportRequest,
  SupportResult,
} from '../models/platform/platform';

/** Own notification feed (`/api/notifications`, any authenticated user). */
@Injectable({ providedIn: 'root' })
export class NotificationsApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  feed(): Observable<NotificationView[]> {
    return this.http.get<NotificationView[]>(`${this.base}/api/notifications`);
  }
  markAllRead(): Observable<void> {
    return this.http.post<void>(`${this.base}/api/notifications/read-all`, {});
  }
}

/** Own announcements feed (`/api/announcements`, any authenticated user). */
@Injectable({ providedIn: 'root' })
export class AnnouncementsApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  mine(): Observable<AnnouncementView[]> {
    return this.http.get<AnnouncementView[]>(`${this.base}/api/announcements`);
  }
  markRead(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/api/announcements/${id}/read`, {});
  }
  markAllRead(): Observable<void> {
    return this.http.post<void>(`${this.base}/api/announcements/read-all`, {});
  }
}

/** Help/support requests (`/api/support`, any authenticated user). */
@Injectable({ providedIn: 'root' })
export class SupportApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  submit(body: SupportRequest): Observable<SupportResult> {
    return this.http.post<SupportResult>(`${this.base}/api/support`, body);
  }
}
