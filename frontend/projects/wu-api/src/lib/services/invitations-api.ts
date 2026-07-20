import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import {
  AcceptInvitationRequest,
  BulkInvitationRequest,
  BulkInvitationResult,
  CreateInvitationRequest,
  InvitationAuthResult,
  InvitationContext,
  InvitationListParams,
  InvitationOptions,
  InvitationPage,
  InvitationResult,
} from '../models/auth/invitations';

@Injectable({ providedIn: 'root' })
export class InvitationsApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  list(options: InvitationListParams = {}): Observable<InvitationPage> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(options)) {
      if (value !== undefined && value !== null && value !== '') params = params.set(key, value);
    }
    return this.http.get<InvitationPage>(`${this.base}/api/invitations`, { params });
  }

  context(token: string): Observable<InvitationContext> {
    return this.http.get<InvitationContext>(`${this.base}/api/invitations/context`, {
      params: { token },
    });
  }

  options(): Observable<InvitationOptions> {
    return this.http.get<InvitationOptions>(`${this.base}/api/invitations/options`);
  }

  create(request: CreateInvitationRequest): Observable<InvitationResult> {
    return this.http.post<InvitationResult>(`${this.base}/api/invitations`, request);
  }

  bulk(request: BulkInvitationRequest): Observable<BulkInvitationResult> {
    return this.http.post<BulkInvitationResult>(`${this.base}/api/invitations/bulk`, request);
  }

  accept(request: AcceptInvitationRequest): Observable<InvitationAuthResult> {
    return this.http.post<InvitationAuthResult>(`${this.base}/api/invitations/accept`, request);
  }

  resend(id: string): Observable<void> {
    return this.http.post<void>(
      `${this.base}/api/invitations/${encodeURIComponent(id)}/resend`,
      {},
    );
  }

  revoke(id: string): Observable<void> {
    return this.http.post<void>(
      `${this.base}/api/invitations/${encodeURIComponent(id)}/revoke`,
      {},
    );
  }

  requestReplacement(token: string): Observable<void> {
    return this.http.post<void>(`${this.base}/api/invitations/replacement-request`, { token });
  }
}
