import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AuthStore } from '@wu/auth';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { EditorDraftService } from './editor-draft.service';

describe('EditorDraftService', () => {
  const user = signal({
    userId: 'owner-1',
    email: 'owner@example.com',
    displayName: 'Owner',
    role: 'ProgramAdministrator',
  });
  let service: EditorDraftService;

  beforeEach(() => {
    sessionStorage.clear();
    user.set({ ...user(), userId: 'owner-1' });
    TestBed.configureTestingModule({
      providers: [{ provide: AuthStore, useValue: { user: user.asReadonly() } }],
    });
    service = TestBed.inject(EditorDraftService);
  });

  afterEach(() => {
    sessionStorage.clear();
    TestBed.resetTestingModule();
  });

  it('restores a draft once and only to the authenticated owner', () => {
    expect(service.save('badges', { name: 'Local badge draft' })).toBe(true);
    user.set({ ...user(), userId: 'different-user' });
    expect(service.take<{ name: string }>('badges')).toBeNull();
    expect(service.save('badges', { name: 'Different owner draft' })).toBe(true);

    user.set({ ...user(), userId: 'owner-1' });
    expect(service.take<{ name: string }>('badges')).toEqual({ name: 'Local badge draft' });
    expect(service.take<{ name: string }>('badges')).toBeNull();

    user.set({ ...user(), userId: 'different-user' });
    expect(service.take<{ name: string }>('badges')).toEqual({ name: 'Different owner draft' });
  });
});
