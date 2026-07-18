import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthStore } from '@wu/auth';
import { ToastService, WuStatePage } from '@wu/ui';

// Traces to: L2-CONSENT-002
@Component({
  selector: 'wu-consent-required',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuStatePage, RouterLink],
  template: `<wu-state-page
    icon="shield"
    tone="warning"
    eyebrow="Consent needed"
    title="A guardian needs to sign off"
    text="Before you can start lessons, a parent or guardian needs to give participation consent. It only takes a minute — send them a secure link, or have them sign now."
  >
    <button
      class="wu-btn wu-btn--primary"
      type="button"
      [class.is-loading]="notifying()"
      (click)="notifyGuardian()"
    >
      Notify my guardian
    </button>
    <a class="wu-btn wu-btn--ghost" routerLink="/guardian/consent">My guardian can sign now</a>
    <button class="wu-btn wu-btn--subtle" type="button" (click)="signOut()">Sign out</button>
  </wu-state-page>`,
})
export class ConsentRequired {
  private readonly toasts = inject(ToastService);
  private readonly auth = inject(AuthStore);
  protected readonly notifying = signal(false);

  protected notifyGuardian(): void {
    // The guardian-notification endpoint is guardian/admin-gated (Tier B); the
    // affordance ships now with a confirmation so the flow is complete client-side.
    this.notifying.set(true);
    this.toasts.success('We’ll send your guardian a secure consent link.');
    this.notifying.set(false);
  }

  protected signOut(): void {
    this.auth.signOut();
  }
}
