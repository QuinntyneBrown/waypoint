import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthStore } from '@waypoint/auth';
import { WIcon, WShell } from '@waypoint/components';
import { GuardianContextService } from '../guardian-context.service';

// Traces to: L2-GUARDIAN-001
@Component({
  selector: 'w-guardian-home',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, RouterLink, WIcon],
  templateUrl: './guardian-home.html',
})
export class GuardianHome {
  protected readonly auth = inject(AuthStore);
  protected readonly context = inject(GuardianContextService);

  constructor() {
    this.context.load();
  }
}
