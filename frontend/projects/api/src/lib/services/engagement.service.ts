import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import {
  AnnouncementResult,
  ComposeAnnouncementRequest,
  ManagedAnnouncement,
  AnnouncementView,
  NotificationView,
  SupportRequest,
  SupportResult,
  SupportScreenshot,
} from '../models/platform/platform';
import {
  BroadcastResult,
  MentorMessageInbox,
  MessageResult,
  MessageView,
  YouthMessageThread,
} from '../models/engagement/messages';
import {
  AttendanceHistoryFilter,
  AttendanceTally,
  AttendanceWrite,
  CalendarSession,
  CreateSessionRequest,
  CreateSessionResult,
  SessionAttendanceSummary,
  SessionSchedulingOptions,
  StaffSessionAttendance,
  YouthAttendanceSummary,
} from '../models/engagement/operations';

/** Staff session scheduling, attendance capture, history, and filtered exports. */
@Injectable({ providedIn: 'root' })
export class StaffOperationsService {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  sessionOptions(): Observable<SessionSchedulingOptions> {
    return this.http.get<SessionSchedulingOptions>(`${this.base}/api/sessions/options`);
  }

  sessions(filter: AttendanceHistoryFilter = {}): Observable<CalendarSession[]> {
    return this.http.get<CalendarSession[]>(`${this.base}/api/sessions`, {
      params: this.params(filter),
    });
  }

  createSession(request: CreateSessionRequest): Observable<CreateSessionResult> {
    return this.http.post<CreateSessionResult>(`${this.base}/api/sessions`, request);
  }

  attendanceRoster(sessionId: string): Observable<StaffSessionAttendance> {
    return this.http.get<StaffSessionAttendance>(
      `${this.base}/api/attendance/${encodeURIComponent(sessionId)}/roster`,
    );
  }

  saveAttendance(write: AttendanceWrite, idempotencyKey: string): Observable<AttendanceTally> {
    return this.http.post<AttendanceTally>(`${this.base}/api/attendance`, write, {
      headers: { 'Idempotency-Key': idempotencyKey },
    });
  }

  sessionHistory(filter: AttendanceHistoryFilter = {}): Observable<SessionAttendanceSummary[]> {
    return this.http.get<SessionAttendanceSummary[]>(
      `${this.base}/api/attendance/history/by-session`,
      { params: this.params(filter) },
    );
  }

  youthHistory(filter: AttendanceHistoryFilter = {}): Observable<YouthAttendanceSummary[]> {
    return this.http.get<YouthAttendanceSummary[]>(`${this.base}/api/attendance/history/youth`, {
      params: this.params(filter),
    });
  }

  exportAttendance(
    format: 'csv' | 'pdf',
    view: 'session' | 'youth',
    filter: AttendanceHistoryFilter = {},
  ): Observable<Blob> {
    return this.http.get(`${this.base}/api/attendance/history/export`, {
      params: this.params(filter).set('format', format).set('view', view),
      responseType: 'blob',
    });
  }

  private params(filter: AttendanceHistoryFilter): HttpParams {
    let params = new HttpParams();
    if (filter.cohortId) params = params.set('cohortId', filter.cohortId);
    if (filter.from) params = params.set('from', filter.from);
    if (filter.to) params = params.set('to', filter.to);
    return params;
  }
}

/** Assigned-mentor messaging (`/api/me/messages`, Youth). */
@Injectable({ providedIn: 'root' })
export class MessagingService {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  mine(): Observable<YouthMessageThread> {
    return this.http.get<YouthMessageThread>(`${this.base}/api/me/messages`);
  }

  mentorInbox(): Observable<MentorMessageInbox> {
    return this.http.get<MentorMessageInbox>(`${this.base}/api/mentor/messages`);
  }

  thread(otherUserId: string): Observable<MessageView[]> {
    return this.http.get<MessageView[]>(
      `${this.base}/api/messages/${encodeURIComponent(otherUserId)}`,
    );
  }

  send(toUserId: string, body: string): Observable<MessageResult> {
    return this.http.post<MessageResult>(`${this.base}/api/messages`, { toUserId, body });
  }

  broadcast(cohortId: string, body: string): Observable<BroadcastResult> {
    return this.http.post<BroadcastResult>(`${this.base}/api/messages/broadcast`, {
      cohortId,
      body,
    });
  }
}

/** Own notification feed (`/api/notifications`, any authenticated user). */
@Injectable({ providedIn: 'root' })
export class NotificationsService {
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
export class AnnouncementsService {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  mine(): Observable<AnnouncementView[]> {
    return this.http.get<AnnouncementView[]>(`${this.base}/api/announcements`);
  }
  compose(request: ComposeAnnouncementRequest): Observable<AnnouncementResult> {
    return this.http.post<AnnouncementResult>(`${this.base}/api/announcements`, request);
  }
  manage(): Observable<ManagedAnnouncement[]> {
    return this.http.get<ManagedAnnouncement[]>(`${this.base}/api/announcements/manage`);
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
export class SupportService {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  submit(body: SupportRequest): Observable<SupportResult> {
    return this.http.post<SupportResult>(`${this.base}/api/support`, body);
  }

  uploadScreenshot(file: File): Observable<SupportScreenshot> {
    const form = new FormData();
    form.append('file', file, file.name);
    return this.http.post<SupportScreenshot>(`${this.base}/api/support/screenshots`, form);
  }

  removeScreenshot(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/api/support/screenshots/${encodeURIComponent(id)}`);
  }
}
