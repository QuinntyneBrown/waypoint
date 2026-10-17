import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { deploymentUrl, WAYPOINT_DEPLOYMENT_URLS, WButton, WStatePage } from '@waypoint/components';

// Traces to: L2-PLATFORM-010
@Component({
  selector: 'w-account-deactivated',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WStatePage, RouterLink, WButton],
  template: `<w-state-page
    icon="alert-circle"
    tone="warning"
    eyebrow="Account inactive"
    title="This account is inactive"
    text="Your account isn’t active right now. Please contact your program director to restore access."
  >
    <a w-button variant="primary" [href]="contactUrl">Contact the director</a>
    <a w-button variant="ghost" routerLink="/auth/sign-in">Back to sign in</a>
  </w-state-page>`,
})
export class AccountDeactivated {
  private readonly deployment = inject(WAYPOINT_DEPLOYMENT_URLS);
  protected readonly contactUrl = deploymentUrl(this.deployment.publicSiteBaseUrl, '/contact');
}
