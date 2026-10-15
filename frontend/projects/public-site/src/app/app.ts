import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { deploymentUrl, WAYPOINT_DEPLOYMENT_URLS } from '@waypoint/components';

@Component({
  selector: 'w-root',
  imports: [RouterLink, RouterOutlet],
  templateUrl: './app.html',
})
export class App {
  private readonly deployment = inject(WAYPOINT_DEPLOYMENT_URLS);
  protected readonly signInUrl = deploymentUrl(this.deployment.portalBaseUrl, '/auth/sign-in');
}
