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
import { MessageResult, MessageView, YouthMessageThread } from '../models/engagement/messages';

/** Assigned-mentor messaging (`/api/me/messages`, Youth). */
@Injectable({ providedIn: 'root' })
export class MessagingApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  mine(): Observable<YouthMessageThread> {
    return this.http.get<YouthMessageThread>(`${this.base}/api/me/messages`);
  }

  thread(otherUserId: string): Observable<MessageView[]> {
    return this.http.get<MessageView[]>(
      `${this.base}/api/messages/${encodeURIComponent(otherUserId)}`,
    );
  }

  send(toUserId: string, body: string): Observable<MessageResult> {
    return this.http.post<MessageResult>(`${this.base}/api/messages`, { toUserId, body });
  }
}

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
