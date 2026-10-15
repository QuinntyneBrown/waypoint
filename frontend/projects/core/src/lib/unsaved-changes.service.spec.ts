import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { UnsavedChangesService } from './unsaved-changes.service';

describe('UnsavedChangesService', () => {
  let service: UnsavedChangesService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(UnsavedChangesService);
  });

  afterEach(() => {
    vi.restoreAllMocks();
    TestBed.resetTestingModule();
  });

  it('offers to save a dirty editor and keeps a cancelled editor in place', () => {
    const save = vi.fn(() => true);
    const confirm = vi
      .spyOn(window, 'confirm')
      .mockReturnValueOnce(false)
      .mockReturnValueOnce(true);

    expect(service.confirmSaveOrStay(true, 'grading', save)).toBe(false);
    expect(save).not.toHaveBeenCalled();
    expect(service.confirmSaveOrStay(true, 'grading', save)).toBe(true);
    expect(save).toHaveBeenCalledOnce();
    expect(confirm.mock.calls[0][0]).toMatch(/unsaved grading.*save.*draft.*cancel/i);
  });

  it('captures a local draft and cancels browser close only while dirty', () => {
    const save = vi.fn();
    const clean = new Event('beforeunload', { cancelable: true }) as BeforeUnloadEvent;
    const dirty = new Event('beforeunload', { cancelable: true }) as BeforeUnloadEvent;

    service.warnBeforeUnload(clean, false, save);
    service.warnBeforeUnload(dirty, true, save);

    expect(clean.defaultPrevented).toBe(false);
    expect(dirty.defaultPrevented).toBe(true);
    expect(save).toHaveBeenCalledOnce();
  });
});
