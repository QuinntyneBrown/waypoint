import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { deploymentUrl, WAYPOINT_DEPLOYMENT_URLS, Button, StatePage } from '@waypoint/components';

// Traces to: L2-PLATFORM-010
@Component({
  selector: 'ph-account-deactivated',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [StatePage, RouterLink, Button],
  templateUrl: './account-deactivated.html',
  styleUrl: './account-deactivated.css',
})
export class AccountDeactivated {
  private readonly deployment = inject(WAYPOINT_DEPLOYMENT_URLS);
  protected readonly contactUrl = deploymentUrl(this.deployment.publicSiteBaseUrl, '/contact');
}
