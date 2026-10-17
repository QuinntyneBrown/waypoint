import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { WButton, WStatePage } from '@waypoint/components';

// Traces to: L2-AUTH-005
@Component({
  selector: 'w-reset-link-invalid',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WStatePage, RouterLink, WButton],
  template: `<w-state-page
    icon="alert-triangle"
    tone="error"
    eyebrow="Reset link"
    title="This link is invalid or expired"
    text="Reset links can only be used once and expire after an hour. Request a fresh link to continue."
  >
    <a w-button variant="primary" routerLink="/auth/forgot-password">Request a new link</a>
    <a w-button variant="ghost" routerLink="/auth/sign-in">Back to sign in</a>
  </w-state-page>`,
})
export class ResetLinkInvalid {}
