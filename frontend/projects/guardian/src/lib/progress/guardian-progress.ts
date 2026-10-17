import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AuthStore } from '@waypoint/auth';
import {
  WShell,
  WCard,
  WCardBody,
  WCardTitle,
  WEmptyState,
  WEmptyStateTitle,
  WEmptyStateText,
  WListItem,
  WListItemBody,
  WListItemTitle,
  WListItemMeta,
} from '@waypoint/components';
import { GuardianContextService } from '../guardian-context.service';

// Traces to: L2-GUARDIAN-001
@Component({
  selector: 'w-guardian-progress',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    WShell,
    DatePipe,
    WCard,
    WCardBody,
    WCardTitle,
    WEmptyState,
    WEmptyStateTitle,
    WEmptyStateText,
    WListItem,
    WListItemBody,
    WListItemTitle,
    WListItemMeta,
  ],
  templateUrl: './guardian-progress.html',
})
export class GuardianProgress {
  protected readonly auth = inject(AuthStore);
  protected readonly context = inject(GuardianContextService);

  constructor() {
    this.context.load();
  }
}
