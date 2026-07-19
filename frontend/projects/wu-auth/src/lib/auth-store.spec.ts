import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AuthResult } from '@wu/api';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { AuthStore } from './auth-store';
import { SessionPersistence } from './session-persistence';
import { SessionStore } from './session-store';

function authResult(overrides: Partial<AuthResult> = {}): AuthResult {
  return {
    accessToken: 'at-1',
    expiresAt: new Date(Date.now() + 3_600_000).toISOString(),
    refreshToken: 'rt-new',
    userId: 'u1',
    email: 'a@b.c',
    displayName: 'A B',
    role: 'Youth',
    ...overrides,
  };
}

describe('AuthStore', () => {
  let auth: AuthStore;
  let http: HttpTestingController;
  let store: SessionStore;
  let persistence: SessionPersistence;

  beforeEach(() => {
    vi.useFakeTimers();
    localStorage.clear();
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    auth = TestBed.inject(AuthStore);
    http = TestBed.inject(HttpTestingController);
    store = TestBed.inject(SessionStore);
    persistence = TestBed.inject(SessionPersistence);
  });

  afterEach(() => {
    http.verify();
    vi.useRealTimers();
  });

  it('signIn adopts the session and persists the refresh token', () => {
    auth.signIn('a@b.c', 'pw', false).subscribe();
    http.expectOne((r) => r.url.endsWith('/api/auth/sign-in')).flush(authResult());

    expect(store.isAuthenticated()).toBe(true);
    expect(store.accessToken()).toBe('at-1');
    expect(persistence.read()?.refreshToken).toBe('rt-new');
    expect(sessionStorage.getItem('wu.session.v1')).not.toBeNull();
  });

  it('refreshToken is single-flight: concurrent callers share one HTTP call', () => {
    persistence.write({
      version: 1,
      remember: false,
      refreshToken: 'rt-old',
      user: { userId: 'u1', email: 'a@b.c', displayName: 'A B', role: 'Youth' },
    });

    const results: AuthResult[] = [];
    auth.refreshToken().subscribe((r) => results.push(r));
    auth.refreshToken().subscribe((r) => results.push(r));

    // expectOne asserts exactly one refresh request was issued for both callers.
    http.expectOne((r) => r.url.endsWith('/api/auth/refresh')).flush(authResult());

    expect(results).toHaveLength(2);
    expect(results[0]).toBe(results[1]);
    expect(store.accessToken()).toBe('at-1');
  });

  it('restore with no stored session resolves false without any HTTP', () => {
    let result: boolean | undefined;
    auth.restore().subscribe((r) => (result = r));
    http.expectNone((r) => r.url.endsWith('/api/auth/refresh'));
    expect(result).toBe(false);
    expect(store.isAuthenticated()).toBe(false);
  });

  it('restore marks the session expired when a stored refresh token is rejected', () => {
    persistence.write({
      version: 1,
      remember: true,
      refreshToken: 'rt-bad',
      user: { userId: 'u1', email: 'a@b.c', displayName: 'A B', role: 'Youth' },
    });

    let result: boolean | undefined;
    auth.restore().subscribe((r) => (result = r));
    http
      .expectOne((r) => r.url.endsWith('/api/auth/refresh'))
      .flush({ title: 'invalid' }, { status: 401, statusText: 'Unauthorized' });

    expect(result).toBe(false);
    expect(store.expiredOnBoot()).toBe(true);
    expect(store.isAuthenticated()).toBe(false);
  });

  it('inactivity expiry waits for server revocation before the hard local transition', () => {
    store.set(authResult());
    const forceExpire = vi.spyOn(auth, 'forceExpire').mockImplementation(() => {});

    auth.expireForInactivity('/youth/lessons/lesson-1/reflection');
    expect(forceExpire).not.toHaveBeenCalled();
    http
      .expectOne((request) => request.url.endsWith('/api/auth/sign-out'))
      .flush(null, { status: 204, statusText: 'No Content' });

    expect(forceExpire).toHaveBeenCalledWith('/youth/lessons/lesson-1/reflection', true);
  });

  it('inactivity expiry falls back to local teardown when revocation hangs', () => {
    store.set(authResult());
    const forceExpire = vi.spyOn(auth, 'forceExpire').mockImplementation(() => {});

    auth.expireForInactivity('/youth/home');
    const request = http.expectOne((item) => item.url.endsWith('/api/auth/sign-out'));
    vi.advanceTimersByTime(1_999);
    expect(forceExpire).not.toHaveBeenCalled();
    vi.advanceTimersByTime(1);
    expect(forceExpire).toHaveBeenCalledWith('/youth/home', true);
    expect(request.cancelled).toBe(true);
    expect(forceExpire).toHaveBeenCalledOnce();
  });

  it('refreshes an expired bearer only to revoke the idle server session', () => {
    store.set(authResult({ expiresAt: new Date(Date.now() - 1).toISOString() }));
    persistence.write({
      version: 1,
      remember: false,
      refreshToken: 'rt-idle',
      user: { userId: 'u1', email: 'a@b.c', displayName: 'A B', role: 'Youth' },
    });
    const forceExpire = vi.spyOn(auth, 'forceExpire').mockImplementation(() => {});

    auth.expireForInactivity('/youth/home');
    http
      .expectOne((request) => request.url.endsWith('/api/auth/refresh'))
      .flush(authResult({ refreshToken: 'rt-rotated' }));
    http
      .expectOne((request) => request.url.endsWith('/api/auth/sign-out'))
      .flush(null, { status: 204, statusText: 'No Content' });

    expect(forceExpire).toHaveBeenCalledWith('/youth/home', true);
  });
});
