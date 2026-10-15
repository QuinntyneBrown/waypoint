import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { API_BASE_URL } from '../api-base-url';
import type { FulfillDataSubjectRequest } from '../models/platform/privacy';
import { PrivacyApi } from './privacy-api';

describe('PrivacyApi', () => {
  let api: PrivacyApi;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: 'https://api.waypoint.test' },
      ],
    });
    api = TestBed.inject(PrivacyApi);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('requests an attachment response from the authenticated server export', () => {
    let received: Blob | null = null;
    api.exportMyData().subscribe((response) => (received = response.body));

    const request = http.expectOne('https://api.waypoint.test/api/me/data/export');
    expect(request.request.method).toBe('GET');
    expect(request.request.responseType).toBe('blob');
    const body = new Blob(['{"schemaVersion":"1.0"}'], { type: 'application/json' });
    request.flush(body, {
      headers: { 'Content-Disposition': 'attachment; filename="waypoint-personal-data.json"' },
    });

    expect(received).toBe(body);
  });

  it('sends explicit paging parameters for request history', () => {
    api.myRequests(2, 25).subscribe();

    const request = http.expectOne((candidate) => candidate.url.endsWith('/api/me/data-requests'));
    expect(request.request.params.get('page')).toBe('2');
    expect(request.request.params.get('pageSize')).toBe('25');
    request.flush({ items: [], page: 2, pageSize: 25, total: 0, canExport: false });
  });

  it('sends verified legacy-record ownership fields without changing their categories', () => {
    const payload: FulfillDataSubjectRequest = {
      resolutionNote: 'Ownership checked against the original signed records.',
      verifiedOwnershipAttested: true,
      verifiedContactRequestIds: ['11111111-1111-1111-1111-111111111111'],
      verifiedVolunteerSignupIds: ['22222222-2222-2222-2222-222222222222'],
      verifiedDonationPledgeIds: ['33333333-3333-3333-3333-333333333333'],
      verifiedApplicantApplicationIds: ['44444444-4444-4444-4444-444444444444'],
      verifiedGuardianApplicationIds: ['55555555-5555-5555-5555-555555555555'],
    };

    api.fulfill('request-id', payload).subscribe();

    const request = http.expectOne(
      'https://api.waypoint.test/api/admin/data-requests/request-id/fulfill',
    );
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);
    request.flush({
      id: 'request-id',
      reference: 'DSR-0001',
      subjectUserId: 'subject-id',
      actionType: 'access',
      status: 'Completed',
      requestedAt: '2026-07-20T00:00:00Z',
    });
  });
});
