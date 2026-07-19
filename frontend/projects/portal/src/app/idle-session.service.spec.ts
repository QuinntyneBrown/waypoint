import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { AuthStore } from '@wu/auth';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { FormDraftService } from './form-draft.service';
import { IDLE_TIMEOUT_MS, IdleSessionService } from './idle-session.service';

describe('IdleSessionService', () => {
  const authenticated = signal(true);
  const auth = {
    isAuthenticated: authenticated.asReadonly(),
    expireForInactivity: vi.fn(),
  };
  const drafts = { captureCurrentForm: vi.fn() };
  let service: IdleSessionService;

  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2026-07-18T12:00:00Z'));
    authenticated.set(true);
    auth.expireForInactivity.mockReset();
    drafts.captureCurrentForm.mockReset();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AuthStore, useValue: auth },
        { provide: FormDraftService, useValue: drafts },
      ],
    });
    service = TestBed.inject(IdleSessionService);
  });

  afterEach(() => {
    service.ngOnDestroy();
    TestBed.resetTestingModule();
    vi.useRealTimers();
  });

  it('resets on deliberate activity and expires once at the exact idle boundary', () => {
    vi.advanceTimersByTime(IDLE_TIMEOUT_MS - 1);
    expect(auth.expireForInactivity).not.toHaveBeenCalled();

    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Shift' }));
    vi.advanceTimersByTime(IDLE_TIMEOUT_MS - 1);
    expect(auth.expireForInactivity).not.toHaveBeenCalled();

    vi.advanceTimersByTime(1);
    expect(drafts.captureCurrentForm).toHaveBeenCalledOnce();
    expect(auth.expireForInactivity).toHaveBeenCalledWith(TestBed.inject(Router).url);

    vi.advanceTimersByTime(IDLE_TIMEOUT_MS);
    expect(auth.expireForInactivity).toHaveBeenCalledOnce();
  });
});
