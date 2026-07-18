import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { SESSION_KEY, SessionPersistence } from './session-persistence';
import { StoredSession } from './session.model';

function make(remember: boolean): StoredSession {
  return {
    version: 1,
    remember,
    refreshToken: 'rt-123',
    user: { userId: 'u1', email: 'a@b.c', displayName: 'A B', role: 'Youth' },
  };
}

describe('SessionPersistence', () => {
  let p: SessionPersistence;

  beforeEach(() => {
    localStorage.clear();
    sessionStorage.clear();
    TestBed.configureTestingModule({});
    p = TestBed.inject(SessionPersistence);
  });
  afterEach(() => {
    localStorage.clear();
    sessionStorage.clear();
  });

  it('writes to localStorage when remember is true', () => {
    p.write(make(true));
    expect(localStorage.getItem(SESSION_KEY)).not.toBeNull();
    expect(sessionStorage.getItem(SESSION_KEY)).toBeNull();
  });

  it('writes to sessionStorage when remember is false', () => {
    p.write(make(false));
    expect(sessionStorage.getItem(SESSION_KEY)).not.toBeNull();
    expect(localStorage.getItem(SESSION_KEY)).toBeNull();
  });

  it('moves the session between storages when remember changes', () => {
    p.write(make(false));
    p.write(make(true));
    expect(localStorage.getItem(SESSION_KEY)).not.toBeNull();
    expect(sessionStorage.getItem(SESSION_KEY)).toBeNull();
  });

  it('reads a valid v1 session and ignores malformed data', () => {
    p.write(make(true));
    expect(p.read()?.refreshToken).toBe('rt-123');
    localStorage.setItem(SESSION_KEY, 'not json');
    expect(p.read()).toBeNull();
  });

  it('clearSession removes the session but preserves wu.draft.* keys', () => {
    p.write(make(true));
    localStorage.setItem('wu.draft.u1.reflection', 'draft');
    p.clearSession();
    expect(p.read()).toBeNull();
    expect(localStorage.getItem('wu.draft.u1.reflection')).toBe('draft');
  });

  it('purgeAll removes every wu.* key including drafts', () => {
    p.write(make(true));
    localStorage.setItem('wu.draft.u1.reflection', 'draft');
    sessionStorage.setItem('wu.something', 'x');
    p.purgeAll();
    expect(localStorage.getItem('wu.draft.u1.reflection')).toBeNull();
    expect(sessionStorage.getItem('wu.something')).toBeNull();
    expect(p.read()).toBeNull();
  });
});
