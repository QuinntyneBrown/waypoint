import { DOCUMENT } from '@angular/common';
import { inject, Injectable, signal } from '@angular/core';

/**
 * Tracks browser connectivity via `navigator.onLine` and the online/offline
 * events. SSR-safe (no-op without a window). Drives the shell offline banner
 * (L2-PLATFORM-012).
 */
@Injectable({ providedIn: 'root' })
export class OfflineService {
  private readonly doc = inject(DOCUMENT);
  private readonly _offline = signal(false);
  readonly isOffline = this._offline.asReadonly();

  constructor() {
    const win = this.doc.defaultView;
    if (!win?.navigator) return;
    this._offline.set(win.navigator.onLine === false);
    win.addEventListener('online', () => this._offline.set(false));
    win.addEventListener('offline', () => this._offline.set(true));
  }
}
