import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthStore } from '@waypoint/auth';
import {
  Button,
  Icon,
  Shell,
  Alert,
  Card,
  CardBody,
  CardTitle,
  EmptyState,
  EmptyStateIcon,
  EmptyStateTitle,
  EmptyStateText,
  Stat,
  StatValue,
  StatLabel,
} from '@waypoint/components';
import { GuardianContextService } from '../guardian-context.service';

// Traces to: L2-GUARDIAN-001
@Component({
  selector: 'w-guardian-home',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    Shell,
    RouterLink,
    Icon,
    Button,
    Alert,
    Card,
    CardBody,
    CardTitle,
    EmptyState,
    EmptyStateIcon,
    EmptyStateTitle,
    EmptyStateText,
    Stat,
    StatValue,
    StatLabel,
  ],
  templateUrl: './guardian-home.html',
  styleUrl: './guardian-home.css',
})
export class GuardianHome {
  protected readonly auth = inject(AuthStore);
  protected readonly context = inject(GuardianContextService);

  constructor() {
    this.context.load();
  }
}
