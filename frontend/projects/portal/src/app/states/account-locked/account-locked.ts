import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { WuStatePage } from '@wu/ui';

// Traces to: L2-AUTH-006
@Component({
  selector: 'wu-account-locked',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuStatePage, RouterLink],
  template: `<wu-state-page
    icon="lock"
    tone="error"
    eyebrow="Account locked"
    title="Too many sign-in attempts"
    [text]="message"
  >
    <a class="wu-btn wu-btn--primary" routerLink="/auth/forgot-password">Reset password</a>
    <a class="wu-btn wu-btn--ghost" routerLink="/support">Contact your mentor</a>
  </wu-state-page>`,
})
export class AccountLocked {
  private readonly doc = inject(DOCUMENT);
  protected readonly message =
    (this.doc.defaultView?.history.state?.['message'] as string | undefined) ??
    'This account is temporarily locked after several failed attempts. Try again in about 15 minutes, or reset your password.';
}
