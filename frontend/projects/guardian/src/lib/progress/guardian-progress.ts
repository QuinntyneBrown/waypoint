import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AuthStore } from '@waypoint/auth';
import {
  Shell,
  Card,
  CardBody,
  CardTitle,
  EmptyState,
  EmptyStateTitle,
  EmptyStateText,
  ListItem,
  ListItemBody,
  ListItemTitle,
  ListItemMeta,
} from '@waypoint/components';
import { GuardianContextService } from '../guardian-context.service';

// Traces to: L2-GUARDIAN-001
@Component({
  selector: 'ph-guardian-progress',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    Shell,
    DatePipe,
    Card,
    CardBody,
    CardTitle,
    EmptyState,
    EmptyStateTitle,
    EmptyStateText,
    ListItem,
    ListItemBody,
    ListItemTitle,
    ListItemMeta,
  ],
  templateUrl: './guardian-progress.html',
  styleUrl: './guardian-progress.css',
})
export class GuardianProgress {
  protected readonly auth = inject(AuthStore);
  protected readonly context = inject(GuardianContextService);

  constructor() {
    this.context.load();
  }
}
