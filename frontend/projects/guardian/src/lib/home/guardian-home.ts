import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthStore } from '@waypoint/auth';
import {
  WButton,
  WIcon,
  WShell,
  WAlert,
  WCard,
  WCardBody,
  WCardTitle,
  WEmptyState,
  WEmptyStateIcon,
  WEmptyStateTitle,
  WEmptyStateText,
  WStat,
  WStatValue,
  WStatLabel,
} from '@waypoint/components';
import { GuardianContextService } from '../guardian-context.service';

// Traces to: L2-GUARDIAN-001
@Component({
  selector: 'w-guardian-home',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    WShell,
    RouterLink,
    WIcon,
    WButton,
    WAlert,
    WCard,
    WCardBody,
    WCardTitle,
    WEmptyState,
    WEmptyStateIcon,
    WEmptyStateTitle,
    WEmptyStateText,
    WStat,
    WStatValue,
    WStatLabel,
  ],
  templateUrl: './guardian-home.html',
})
export class GuardianHome {
  protected readonly auth = inject(AuthStore);
  protected readonly context = inject(GuardianContextService);

  constructor() {
    this.context.load();
  }
}
