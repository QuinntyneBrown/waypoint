import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { maskEmail, PasswordResetApi } from '@wu/api';
import { ToastService, WuStatePage } from '@wu/ui';

// Traces to: L2-AUTH-005
@Component({
  selector: 'wu-reset-link-sent',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuStatePage, RouterLink],
  template: `<wu-state-page
    icon="mail"
    tone="info"
    eyebrow="Reset link sent"
    title="Check your email"
    [text]="bodyText"
  >
    <button class="wu-btn wu-btn--ghost" type="button" (click)="resend()">Resend link</button>
    <a class="wu-btn wu-btn--primary" routerLink="/auth/sign-in">Back to sign in</a>
  </wu-state-page>`,
})
export class ResetLinkSent {
  private readonly doc = inject(DOCUMENT);
  private readonly api = inject(PasswordResetApi);
  private readonly toasts = inject(ToastService);
  private readonly email = (this.doc.defaultView?.history.state?.['email'] as string) ?? '';

  protected readonly bodyText = this.email
    ? `If an account exists for ${maskEmail(this.email)}, we’ve emailed a link to reset your password. The link expires in 1 hour.`
    : 'If an account exists for that address, we’ve emailed a link to reset your password. The link expires in 1 hour.';

  protected readonly resending = signal(false);

  protected resend(): void {
    if (!this.email || this.resending()) return;
    this.resending.set(true);
    this.api.requestReset({ email: this.email }).subscribe({
      next: () => {
        this.resending.set(false);
        this.toasts.success('Reset link sent again.');
      },
      error: () => {
        this.resending.set(false);
        this.toasts.error('Couldn’t resend just now. Please try again.');
      },
    });
  }
}
