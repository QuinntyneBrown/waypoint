import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AuthStore } from '@wu/auth';
import { WuShell } from '@wu/ui';
import { GuardianContextService } from '../guardian-context.service';

// Traces to: L2-GUARDIAN-001
@Component({
  selector: 'wu-guardian-progress',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, DatePipe],
  templateUrl: './guardian-progress.html',
})
export class GuardianProgress {
  protected readonly auth = inject(AuthStore);
  protected readonly context = inject(GuardianContextService);

  constructor() {
    this.context.load();
  }
}
