import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ApiError, ConsentApi } from '@wu/api';
import { AuthStore } from '@wu/auth';
import { WuStatePage } from '@wu/ui';

// Traces to: L2-CONSENT-002
@Component({
  selector: 'wu-consent-required',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuStatePage],
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
      [disabled]="notifying()"
    >
      Notify my guardian
    </button>
    <button class="wu-btn wu-btn--ghost" type="button" (click)="guardianSignNow()">
      My guardian can sign now
    </button>
    <button class="wu-btn wu-btn--subtle" type="button" (click)="signOut()">Sign out</button>
    @if (deliveryMessage(); as message) {
      <p class="wu-alert wu-alert--success" role="status" aria-live="polite">{{ message }}</p>
    }
    @if (errorMessage(); as error) {
      <p class="wu-alert wu-alert--error" role="alert">{{ error }}</p>
    }
  </wu-state-page>`,
})
export class ConsentRequired {
  private readonly api = inject(ConsentApi);
  private readonly auth = inject(AuthStore);
  protected readonly notifying = signal(false);
  protected readonly deliveryMessage = signal<string | null>(null);
  protected readonly errorMessage = signal<string | null>(null);

  protected notifyGuardian(): void {
    if (this.notifying()) return;
    this.notifying.set(true);
    this.deliveryMessage.set(null);
    this.errorMessage.set(null);
    this.api.requestGuardian().subscribe({
      next: (delivery) => {
        this.notifying.set(false);
        this.deliveryMessage.set(
          `A secure consent link was sent by ${delivery.channel} to ${delivery.destination}.`,
        );
      },
      error: (error: unknown) => {
        this.notifying.set(false);
        this.errorMessage.set(ApiError.from(error).title);
      },
    });
  }

  protected guardianSignNow(): void {
    this.auth.signOut('/auth/sign-in?returnUrl=%2Fguardian%2Fconsent');
  }

  protected signOut(): void {
    this.auth.signOut();
  }
}
