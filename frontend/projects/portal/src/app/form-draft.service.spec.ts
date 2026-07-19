import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AuthStore } from '@wu/auth';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { FormDraftService } from './form-draft.service';

describe('FormDraftService', () => {
  const user = signal({
    userId: 'owner-1',
    email: 'owner@example.com',
    displayName: 'Owner',
    role: 'Youth',
  });
  let drafts: FormDraftService;

  beforeEach(() => {
    sessionStorage.clear();
    user.set({
      userId: 'owner-1',
      email: 'owner@example.com',
      displayName: 'Owner',
      role: 'Youth',
    });
    document.body.innerHTML = `
      <form>
        <textarea id="reflection-text">private draft</textarea>
        <input type="radio" formcontrolname="mood" value="thoughtful" checked>
        <input type="checkbox" formcontrolname="shareWithMentor" checked>
        <input id="password" type="password" value="never persist">
        <input id="evidence" type="file">
      </form>`;
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthStore, useValue: { user: user.asReadonly() } },
        { provide: Router, useValue: { url: '/youth/lessons/lesson-1/reflection' } },
      ],
    });
    drafts = TestBed.inject(FormDraftService);
  });

  afterEach(() => {
    document.body.replaceChildren();
    sessionStorage.clear();
    TestBed.resetTestingModule();
  });

  it('restores safe controls only for the user who owned the captured route', () => {
    drafts.captureCurrentForm();
    const stored = sessionStorage.getItem('wu.transient-form-draft') ?? '';
    expect(stored).toContain('owner-1');
    expect(stored).toContain('private draft');
    expect(stored).not.toContain('never persist');
    expect(stored).not.toContain('evidence');

    const text = document.querySelector<HTMLTextAreaElement>('#reflection-text')!;
    const mood = document.querySelector<HTMLInputElement>('[formcontrolname="mood"]')!;
    const share = document.querySelector<HTMLInputElement>('[formcontrolname="shareWithMentor"]')!;
    text.value = '';
    mood.checked = false;
    share.checked = false;
    user.set({ ...user(), userId: 'different-user' });

    expect(drafts.restoreAndClear()).toBe(false);
    expect(text.value).toBe('');
    expect(sessionStorage.getItem('wu.transient-form-draft')).not.toBeNull();

    user.set({ ...user(), userId: 'owner-1' });
    expect(drafts.restoreAndClear()).toBe(true);
    expect(text.value).toBe('private draft');
    expect(mood.checked).toBe(true);
    expect(share.checked).toBe(true);
    expect(sessionStorage.getItem('wu.transient-form-draft')).toBeNull();
  });
});
