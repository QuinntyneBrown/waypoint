import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { maintenanceInterceptor } from './maintenance.interceptor';

describe('maintenanceInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([maintenanceInterceptor])),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    });
    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
  });

  it('routes to /maintenance on a 503 response', () => {
    const nav = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    http.get('/api/anything').subscribe({ next: () => {}, error: () => {} });
    httpMock
      .expectOne('/api/anything')
      .flush({ title: 'maintenance' }, { status: 503, statusText: 'Service Unavailable' });
    expect(nav).toHaveBeenCalledWith('/maintenance');
  });

  it('ignores non-503 errors', () => {
    const nav = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    http.get('/api/anything').subscribe({ next: () => {}, error: () => {} });
    httpMock
      .expectOne('/api/anything')
      .flush({ title: 'nope' }, { status: 500, statusText: 'Server Error' });
    expect(nav).not.toHaveBeenCalled();
  });
});
