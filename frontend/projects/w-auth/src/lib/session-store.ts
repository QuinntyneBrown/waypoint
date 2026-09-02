import { computed, Injectable, signal } from '@angular/core';
import { AuthResult } from '@w/api';
import { SessionUser } from './session.model';

/**
 * In-memory session state. The access token lives ONLY here (never persisted);
 * a reload re-mints it from the stored refresh token. Signal-based so guards,
 * the shell, and the interceptor all react without zones.
 */
@Injectable({ providedIn: 'root' })
export class SessionStore {
  private readonly _accessToken = signal<string | null>(null);
  /** Access-token expiry as epoch ms, captured once at receipt (no wall-clock re-derivation). */
  private readonly _expiresAt = signal<number | null>(null);
  private readonly _user = signal<SessionUser | null>(null);
  /** True when a stored session existed at boot but could not be restored (expired/revoked). */
  private readonly _expiredOnBoot = signal(false);

  readonly user = this._user.asReadonly();
  readonly isAuthenticated = computed(() => this._user() !== null);
  readonly role = computed(() => this._user()?.role ?? null);
  readonly displayName = computed(() => this._user()?.displayName ?? '');
  readonly expiredOnBoot = this._expiredOnBoot.asReadonly();

  /** Mark that a previously stored session was found invalid at boot. */
  markExpired(): void {
    this._expiredOnBoot.set(true);
  }

  accessToken(): string | null {
    return this._accessToken();
  }
  expiresAt(): number | null {
    return this._expiresAt();
  }

  /** Adopt a fresh AuthResult (sign-in, refresh, or boot restore). */
  set(auth: AuthResult): void {
    this._accessToken.set(auth.accessToken);
    this._expiresAt.set(Date.parse(auth.expiresAt));
    this._user.set({
      userId: auth.userId,
      email: auth.email,
      displayName: auth.displayName,
      role: auth.role,
    });
  }

  clear(): void {
    this._accessToken.set(null);
    this._expiresAt.set(null);
    this._user.set(null);
  }
}
