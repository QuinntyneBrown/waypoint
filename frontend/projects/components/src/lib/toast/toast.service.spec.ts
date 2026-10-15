import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ToastService } from './toast.service';

describe('ToastService', () => {
  let toasts: ToastService;

  beforeEach(() => {
    vi.useFakeTimers();
    TestBed.configureTestingModule({});
    toasts = TestBed.inject(ToastService);
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('adds a toast with the given message and type', () => {
    toasts.success('Saved', { title: 'Done' });
    const list = toasts.toasts();
    expect(list).toHaveLength(1);
    expect(list[0]).toMatchObject({ message: 'Saved', type: 'success', title: 'Done' });
  });

  it('auto-dismisses after the duration', () => {
    toasts.show('bye', { duration: 1000 });
    expect(toasts.toasts()).toHaveLength(1);
    vi.advanceTimersByTime(999);
    expect(toasts.toasts()).toHaveLength(1);
    vi.advanceTimersByTime(1);
    expect(toasts.toasts()).toHaveLength(0);
  });

  it('keeps sticky toasts (duration 0) until dismissed', () => {
    const id = toasts.error('stuck', { duration: 0 });
    vi.advanceTimersByTime(100_000);
    expect(toasts.toasts()).toHaveLength(1);
    toasts.dismiss(id);
    expect(toasts.toasts()).toHaveLength(0);
  });

  it('dismiss clears the pending auto-dismiss timer', () => {
    const id = toasts.info('hi', { duration: 1000 });
    toasts.dismiss(id);
    expect(toasts.toasts()).toHaveLength(0);
    // advancing time must not throw or re-remove
    vi.advanceTimersByTime(2000);
    expect(toasts.toasts()).toHaveLength(0);
  });

  it('clear removes all toasts', () => {
    toasts.show('a');
    toasts.show('b');
    toasts.clear();
    expect(toasts.toasts()).toHaveLength(0);
  });
});
