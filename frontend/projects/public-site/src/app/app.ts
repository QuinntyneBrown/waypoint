import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { deploymentUrl, WORD_UP_DEPLOYMENT_URLS } from '@wu/ui';

@Component({
  selector: 'wu-root',
  imports: [RouterLink, RouterOutlet],
  templateUrl: './app.html',
})
export class App {
  private readonly deployment = inject(WORD_UP_DEPLOYMENT_URLS);
  protected readonly signInUrl = deploymentUrl(this.deployment.portalBaseUrl, '/auth/sign-in');
}
