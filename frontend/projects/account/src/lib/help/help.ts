import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthStore } from '@waypoint/auth';
import {
  Button,
  Icon,
  Shell,
  TextField,
  Card,
  CardBody,
  CardTitle,
  EmptyState,
  EmptyStateIcon,
  EmptyStateTitle,
  EmptyStateText,
} from '@waypoint/components';
import { shellRole } from '@waypoint/core';
import { helpTopicsFor } from './help-content';

// Traces to: L2-PLATFORM-005
@Component({
  selector: 'ph-help',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    RouterLink,
    Icon,
    Shell,
    Button,
    TextField,
    Card,
    CardBody,
    CardTitle,
    EmptyState,
    EmptyStateIcon,
    EmptyStateTitle,
    EmptyStateText,
  ],
  templateUrl: './help.html',
  styleUrl: './help.css',
})
export class HelpPage {
  protected readonly auth = inject(AuthStore);
  protected readonly role = computed(() => shellRole(this.auth.role()));
  protected readonly query = signal('');
  protected readonly topics = computed(() => helpTopicsFor(this.role(), this.query()));

  protected search(event: Event): void {
    this.query.set((event.target as HTMLInputElement).value);
  }
}
