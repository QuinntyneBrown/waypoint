import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { WuStatePage } from '@wu/ui';

// Traces to: L2-AUTH-005
@Component({
  selector: 'wu-reset-link-invalid',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuStatePage, RouterLink],
  template: `<wu-state-page
    icon="alert-triangle"
    tone="error"
    eyebrow="Reset link"
    title="This link is invalid or expired"
    text="Reset links can only be used once and expire after an hour. Request a fresh link to continue."
  >
    <a class="wu-btn wu-btn--primary" routerLink="/auth/forgot-password">Request a new link</a>
    <a class="wu-btn wu-btn--ghost" routerLink="/auth/sign-in">Back to sign in</a>
  </wu-state-page>`,
})
export class ResetLinkInvalid {}
