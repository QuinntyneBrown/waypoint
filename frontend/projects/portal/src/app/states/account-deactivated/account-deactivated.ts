import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { deploymentUrl, WORD_UP_DEPLOYMENT_URLS, WuStatePage } from '@wu/ui';

// Traces to: L2-PLATFORM-010
@Component({
  selector: 'wu-account-deactivated',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuStatePage, RouterLink],
  template: `<wu-state-page
    icon="alert-circle"
    tone="warning"
    eyebrow="Account inactive"
    title="This account is inactive"
    text="Your account isn’t active right now. Please contact your program director to restore access."
  >
    <a class="wu-btn wu-btn--primary" [href]="contactUrl">Contact the director</a>
    <a class="wu-btn wu-btn--ghost" routerLink="/auth/sign-in">Back to sign in</a>
  </wu-state-page>`,
})
export class AccountDeactivated {
  private readonly deployment = inject(WORD_UP_DEPLOYMENT_URLS);
  protected readonly contactUrl = deploymentUrl(this.deployment.publicSiteBaseUrl, '/contact');
}
