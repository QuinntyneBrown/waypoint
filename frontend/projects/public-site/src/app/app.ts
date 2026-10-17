import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import {
  deploymentUrl,
  WAYPOINT_DEPLOYMENT_URLS,
  WButton,
  WSkipLink,
  WMarketingNav,
  WMarketingNavLinks,
  WBrand,
  WFooter,
} from '@waypoint/components';

@Component({
  selector: 'w-root',
  imports: [
    RouterLink,
    RouterOutlet,
    WButton,
    WSkipLink,
    WMarketingNav,
    WMarketingNavLinks,
    WBrand,
    WFooter,
  ],
  templateUrl: './app.html',
})
export class App {
  private readonly deployment = inject(WAYPOINT_DEPLOYMENT_URLS);
  protected readonly signInUrl = deploymentUrl(this.deployment.portalBaseUrl, '/auth/sign-in');
}
