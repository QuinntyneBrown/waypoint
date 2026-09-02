import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import {
  AdminDataSubjectRequestPage,
  CreateDataSubjectRequest,
  DataSubjectRequestResult,
  FulfillDataSubjectRequest,
  MyData,
  MyDataSubjectRequestPage,
} from '../models/platform/privacy';

@Injectable({ providedIn: 'root' })
export class PrivacyApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  myData(): Observable<MyData> {
    return this.http.get<MyData>(`${this.base}/api/me/data`);
  }

  myRequests(page = 1, pageSize = 20): Observable<MyDataSubjectRequestPage> {
    return this.http.get<MyDataSubjectRequestPage>(`${this.base}/api/me/data-requests`, {
      params: { page, pageSize },
    });
  }

  exportMyData(): Observable<HttpResponse<Blob>> {
    return this.http.get(`${this.base}/api/me/data/export`, {
      observe: 'response',
      responseType: 'blob',
    });
  }

  request(request: CreateDataSubjectRequest): Observable<DataSubjectRequestResult> {
    return this.http.post<DataSubjectRequestResult>(`${this.base}/api/me/data-requests`, request);
  }

  adminRequests(status = '', page = 1, pageSize = 25): Observable<AdminDataSubjectRequestPage> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (status) params = params.set('status', status);
    return this.http.get<AdminDataSubjectRequestPage>(`${this.base}/api/admin/data-requests`, {
      params,
    });
  }

  fulfill(id: string, request: FulfillDataSubjectRequest): Observable<DataSubjectRequestResult> {
    return this.http.post<DataSubjectRequestResult>(
      `${this.base}/api/admin/data-requests/${encodeURIComponent(id)}/fulfill`,
      request,
    );
  }
}
