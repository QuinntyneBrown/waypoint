import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';
import { OfflineService } from './offline.service';

describe('OfflineService', () => {
  it('starts online and reacts to offline/online events', () => {
    const service = TestBed.inject(OfflineService);
    expect(service.isOffline()).toBe(false);

    window.dispatchEvent(new Event('offline'));
    expect(service.isOffline()).toBe(true);

    window.dispatchEvent(new Event('online'));
    expect(service.isOffline()).toBe(false);
  });
});
