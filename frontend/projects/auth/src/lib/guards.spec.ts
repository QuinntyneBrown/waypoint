import { TestBed } from '@angular/core/testing';
import { provideRouter, RouterStateSnapshot, UrlTree } from '@angular/router';
import { AuthResult } from '@waypoint/api';
import { beforeEach, describe, expect, it } from 'vitest';
import { authGuard, guestGuard } from './guards';
import { SessionStore } from './session-store';

function authResult(role: string): AuthResult {
  return {
    accessToken: 'at',
    expiresAt: new Date(Date.now() + 3_600_000).toISOString(),
    refreshToken: 'rt',
    userId: 'u1',
    email: 'a@b.c',
    displayName: 'A B',
    role,
  };
}

const state = (url: string) => ({ url }) as RouterStateSnapshot;
const route = {} as never;

describe('authGuard', () => {
  let store: SessionStore;
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
    store = TestBed.inject(SessionStore);
  });

  it('allows an authenticated user', () => {
    store.set(authResult('Youth'));
    const result = TestBed.runInInjectionContext(() => authGuard(route, state('/youth/home')));
    expect(result).toBe(true);
  });

  it('redirects an unauthenticated user to sign-in with a returnUrl', () => {
    const result = TestBed.runInInjectionContext(() =>
      authGuard(route, state('/youth/home')),
    ) as UrlTree;
    expect(result).toBeInstanceOf(UrlTree);
    expect(result.toString()).toContain('/auth/sign-in');
    expect(result.toString()).toContain('returnUrl');
  });

  it('routes a boot-expired session to session-expired instead of sign-in', () => {
    store.markExpired();
    const result = TestBed.runInInjectionContext(() =>
      authGuard(route, state('/youth/home')),
    ) as UrlTree;
    expect(result.toString()).toContain('/session-expired');
  });
});

describe('guestGuard', () => {
  let store: SessionStore;
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
    store = TestBed.inject(SessionStore);
  });

  it('allows an unauthenticated visitor onto the auth screens', () => {
    expect(TestBed.runInInjectionContext(() => guestGuard(route, state('/auth/sign-in')))).toBe(
      true,
    );
  });

  it('bounces a signed-in mentor to their role home', () => {
    store.set(authResult('Mentor'));
    const result = TestBed.runInInjectionContext(() =>
      guestGuard(route, state('/auth/sign-in')),
    ) as UrlTree;
    expect(result.toString()).toContain('/mentor/dashboard');
  });
});
