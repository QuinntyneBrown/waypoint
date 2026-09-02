import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { deploymentUrl, WAYPOINT_DEPLOYMENT_URLS, WStatePage } from '@w/ui';

// Traces to: L2-AUTH-006
@Component({
  selector: 'w-account-locked',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WStatePage, RouterLink],
  template: `<w-state-page
    icon="lock"
    tone="error"
    eyebrow="Account locked"
    title="Too many sign-in attempts"
    [text]="message"
  >
    <a class="w-btn w-btn--primary" routerLink="/auth/forgot-password">Reset password</a>
    <a class="w-btn w-btn--ghost" [href]="contactUrl">Contact your mentor</a>
  </w-state-page>`,
})
export class AccountLocked {
  private readonly doc = inject(DOCUMENT);
  private readonly deployment = inject(WAYPOINT_DEPLOYMENT_URLS);
  protected readonly contactUrl = deploymentUrl(this.deployment.publicSiteBaseUrl, '/contact');
  protected readonly message =
    (this.doc.defaultView?.history.state?.['message'] as string | undefined) ??
    'This account is temporarily locked after several failed attempts. Try again in about 15 minutes, or reset your password.';
}
