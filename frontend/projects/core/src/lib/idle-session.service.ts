import { DOCUMENT } from '@angular/common';
import { effect, inject, Injectable, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { AuthStore } from '@waypoint/auth';
import { FormDraftService } from './form-draft.service';

export const IDLE_TIMEOUT_MS = 30 * 60 * 1000;

/**
 * Tracks deliberate browser activity for authenticated users. When the idle boundary is reached,
 * the current safe form is captured synchronously before the auth store revokes and tears down the
 * session. Background-tab timer throttling is handled by re-checking elapsed wall time on activity.
 */
@Injectable({ providedIn: 'root' })
export class IdleSessionService implements OnDestroy {
  private readonly document = inject(DOCUMENT);
  private readonly auth = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly drafts = inject(FormDraftService);
  private readonly window = this.document.defaultView;
  private readonly activityEvents: (keyof DocumentEventMap)[] = [
    'pointerdown',
    'keydown',
    'touchstart',
    'scroll',
  ];
  private timer: ReturnType<typeof setTimeout> | null = null;
  private lastActivityAt = 0;
  private expiring = false;

  private readonly activityListener = (): void => {
    if (!this.auth.isAuthenticated() || this.expiring) return;
    const now = Date.now();
    if (this.lastActivityAt > 0 && now - this.lastActivityAt >= IDLE_TIMEOUT_MS) {
      this.expire();
      return;
    }
    this.lastActivityAt = now;
    this.arm();
  };

  constructor() {
    for (const event of this.activityEvents) {
      this.document.addEventListener(event, this.activityListener, { passive: true });
    }
    effect(() => {
      if (this.auth.isAuthenticated()) {
        this.expiring = false;
        this.lastActivityAt = Date.now();
        this.arm();
      } else {
        this.stop();
      }
    });
  }

  ngOnDestroy(): void {
    this.stop();
    for (const event of this.activityEvents) {
      this.document.removeEventListener(event, this.activityListener);
    }
  }

  private arm(): void {
    this.stop();
    if (!this.window || !this.auth.isAuthenticated()) return;
    const remaining = Math.max(0, IDLE_TIMEOUT_MS - (Date.now() - this.lastActivityAt));
    this.timer = this.window.setTimeout(() => this.expire(), remaining);
  }

  private expire(): void {
    if (this.expiring || !this.auth.isAuthenticated()) return;
    this.expiring = true;
    this.stop();
    this.drafts.captureCurrentForm();
    this.auth.expireForInactivity(this.router.url);
  }

  private stop(): void {
    if (this.timer !== null) {
      this.window?.clearTimeout(this.timer);
      this.timer = null;
    }
  }
}
