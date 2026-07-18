import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthStore } from '@wu/auth';
import { WuShell } from '@wu/ui';
import { GuardianContextService } from '../guardian-context.service';

// Traces to: L2-GUARDIAN-001
@Component({
  selector: 'wu-guardian-home',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, RouterLink],
  templateUrl: './guardian-home.html',
})
export class GuardianHome {
  protected readonly auth = inject(AuthStore);
  protected readonly context = inject(GuardianContextService);

  constructor() {
    this.context.load();
  }
}
