import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import {
  deploymentUrl,
  WAYPOINT_DEPLOYMENT_URLS,
  Button,
  SkipLink,
  MarketingNav,
  MarketingNavLinks,
  Brand,
  Footer,
} from '@waypoint/components';

@Component({
  selector: 'ph-root',
  imports: [
    RouterLink,
    RouterOutlet,
    Button,
    SkipLink,
    MarketingNav,
    MarketingNavLinks,
    Brand,
    Footer,
  ],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  private readonly deployment = inject(WAYPOINT_DEPLOYMENT_URLS);
  protected readonly signInUrl = deploymentUrl(this.deployment.portalBaseUrl, '/auth/sign-in');
}
