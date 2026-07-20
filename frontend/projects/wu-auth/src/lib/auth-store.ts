import { DOCUMENT } from '@angular/common';
import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { AcceptInvitationRequest, ApiError, AuthApi, AuthResult, InvitationsApi } from '@wu/api';
import {
  catchError,
  finalize,
  map,
  Observable,
  of,
  shareReplay,
  switchMap,
  take,
  tap,
  throwError,
  timeout,
} from 'rxjs';
import { SessionPersistence } from './session-persistence';
import { SessionStore } from './session-store';

/** Refresh this many ms before the access token actually expires. */
const REFRESH_LEAD_MS = 120_000;
const MIN_REFRESH_DELAY_MS = 5_000;

/**
 * Session orchestration: sign-in, single-flight silent refresh with rotation,
 * two-grade teardown (explicit sign-out vs forced expiry), and boot restore.
 * Guards/directives stay presentational — the API response is the authority.
 */
@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly api = inject(AuthApi);
  private readonly invitations = inject(InvitationsApi);
  private readonly store = inject(SessionStore);
  private readonly persistence = inject(SessionPersistence);
  private readonly router = inject(Router);
  private readonly doc = inject(DOCUMENT);

  private refresh$: Observable<AuthResult> | null = null;
  private timer: ReturnType<typeof setTimeout> | null = null;

  readonly user = this.store.user;
  readonly isAuthenticated = this.store.isAuthenticated;
  readonly role = this.store.role;

  signIn(email: string, password: string, remember: boolean): Observable<AuthResult> {
    return this.api.signIn({ email, password }).pipe(tap((auth) => this.adopt(auth, remember)));
  }

  acceptInvitation(request: AcceptInvitationRequest): Observable<AuthResult> {
    return this.invitations.accept(request).pipe(tap((auth) => this.adopt(auth, false)));
  }

  /**
   * Single-flight refresh: concurrent callers (interceptor 401s, scheduled
   * timer) share one HTTP call and one rotation.
   */
  refreshToken(): Observable<AuthResult> {
    if (this.refresh$) return this.refresh$;
    const stored = this.persistence.read();
    if (!stored?.refreshToken) {
      return throwError(() => new ApiError(401, 'Your session is no longer valid.'));
    }
    this.refresh$ = this.api.refresh({ refreshToken: stored.refreshToken }).pipe(
      tap((auth) => this.adopt(auth, stored.remember)),
      finalize(() => (this.refresh$ = null)),
      shareReplay(1),
    );
    return this.refresh$;
  }

  /** Explicit sign-out: revoke, purge ALL wu.* (drafts too), then hard reload. */
  signOut(destination = '/auth/sign-in'): void {
    this.clearTimer();
    this.revokeThen(() => {
      this.store.clear();
      this.persistence.purgeAll();
      this.doc.defaultView?.location.assign(destination);
    });
  }

  /** Forced expiry (refresh failed / family revoked): purge session but keep drafts. */
  forceExpire(returnUrl?: string, hardTransition = false): void {
    this.clearTimer();
    this.store.clear();
    this.persistence.clearSession();
    const destination = this.router.createUrlTree(['/session-expired'], {
      queryParams: returnUrl ? { returnUrl } : undefined,
    });
    if (hardTransition && this.doc.defaultView) {
      this.doc.defaultView.location.assign(destination.toString());
    } else {
      void this.router.navigateByUrl(destination);
    }
  }

  /**
   * Inactivity expiry revokes the server session while the bearer token is still available, then
   * performs the draft-preserving local teardown. A short fallback ensures a disconnected device is
   * never kept signed in locally.
   */
  expireForInactivity(returnUrl: string): void {
    const recoveryUrl =
      returnUrl.startsWith('/') && !returnUrl.startsWith('/auth/') ? returnUrl : undefined;
    this.clearTimer();
    this.revokeThen(() => this.forceExpire(recoveryUrl, true));
  }

  /**
   * Boot restore: a stored refresh token re-mints the session. No stored session
   * → unauthenticated (guard routes to sign-in). A stored session that fails to
   * restore (expired/revoked → 401) is marked so the guard routes to
   * `/session-expired` and drafts are preserved.
   */
  restore(): Observable<boolean> {
    const stored = this.persistence.read();
    if (!stored?.refreshToken) return of(false);
    return this.refreshToken().pipe(
      map(() => true),
      catchError(() => {
        this.store.markExpired();
        this.store.clear();
        this.persistence.clearSession();
        return of(false);
      }),
    );
  }

  private adopt(auth: AuthResult, remember: boolean): void {
    this.store.set(auth);
    this.persistence.write({
      version: 1,
      refreshToken: auth.refreshToken,
      user: {
        userId: auth.userId,
        email: auth.email,
        displayName: auth.displayName,
        role: auth.role,
      },
      remember,
    });
    this.scheduleRefresh(Date.parse(auth.expiresAt));
  }

  private scheduleRefresh(expiresAtMs: number): void {
    this.clearTimer();
    if (!Number.isFinite(expiresAtMs)) return;
    const delay = Math.max(expiresAtMs - Date.now() - REFRESH_LEAD_MS, MIN_REFRESH_DELAY_MS);
    this.timer = setTimeout(() => {
      this.refreshToken().subscribe({ next: () => {}, error: () => this.forceExpire() });
    }, delay);
  }

  private clearTimer(): void {
    if (this.timer) {
      clearTimeout(this.timer);
      this.timer = null;
    }
  }

  private revokeThen(teardown: () => void): void {
    this.revokeSession()
      .pipe(
        timeout(2_000),
        catchError(() => of(undefined)),
        take(1),
      )
      .subscribe(() => teardown());
  }

  private revokeSession(): Observable<void> {
    const token = this.store.accessToken();
    const expiresAt = this.store.expiresAt();
    if (token && (expiresAt === null || expiresAt > Date.now())) {
      return this.api.signOut();
    }
    if (this.persistence.read()?.refreshToken) {
      return this.refreshToken().pipe(switchMap(() => this.api.signOut()));
    }
    return of(undefined);
  }
}
