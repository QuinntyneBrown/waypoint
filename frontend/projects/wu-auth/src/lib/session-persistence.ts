import { Injectable } from '@angular/core';
import { StoredSession } from './session.model';

export const SESSION_KEY = 'wu.session.v1';
export const WU_KEY_PREFIX = 'wu.';
export const DRAFT_PREFIX = 'wu.draft.';

/**
 * Persists the session envelope across reloads. Access tokens are NEVER stored
 * here — only the refresh token + user snapshot, so a reload re-mints the access
 * token via refresh. "Remember me" chooses the durability:
 *
 * - checked   → localStorage (survives browser restart, ~30-day refresh window)
 * - unchecked → sessionStorage (cleared when the browser session ends, so the
 *   session is not silently restored afterwards — L2-AUTH-001 AC4)
 *
 * Isolating the storage choice here keeps the AC4 reading in one place.
 */
@Injectable({ providedIn: 'root' })
export class SessionPersistence {
  private get local(): Storage | null {
    try {
      return globalThis.localStorage ?? null;
    } catch {
      return null;
    }
  }
  private get session(): Storage | null {
    try {
      return globalThis.sessionStorage ?? null;
    } catch {
      return null;
    }
  }

  read(): StoredSession | null {
    const raw = this.local?.getItem(SESSION_KEY) ?? this.session?.getItem(SESSION_KEY) ?? null;
    if (!raw) return null;
    try {
      const parsed = JSON.parse(raw) as StoredSession;
      return parsed?.version === 1 && parsed.refreshToken ? parsed : null;
    } catch {
      return null;
    }
  }

  write(stored: StoredSession): void {
    const value = JSON.stringify(stored);
    const target = stored.remember ? this.local : this.session;
    const other = stored.remember ? this.session : this.local;
    other?.removeItem(SESSION_KEY);
    target?.setItem(SESSION_KEY, value);
  }

  /** Forced-expiry teardown: drop the session but keep `wu.draft.*` for recovery. */
  clearSession(): void {
    this.local?.removeItem(SESSION_KEY);
    this.session?.removeItem(SESSION_KEY);
  }

  /** Explicit sign-out teardown: purge every `wu.*` key, drafts included. */
  purgeAll(): void {
    for (const storage of [this.local, this.session]) {
      if (!storage) continue;
      const keys: string[] = [];
      for (let i = 0; i < storage.length; i++) {
        const key = storage.key(i);
        if (key?.startsWith(WU_KEY_PREFIX)) keys.push(key);
      }
      for (const key of keys) storage.removeItem(key);
    }
  }
}
