import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiError, LearningService, YouthHomeView } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  Button,
  Icon,
  Shell,
  Alert,
  Card,
  CardBody,
  CardTitle,
  Scripture,
  ScriptureRef,
  Badge,
  EmptyState,
  EmptyStateIcon,
  EmptyStateTitle,
  EmptyStateText,
  Progress,
} from '@waypoint/components';

// Traces to: L2-LEARN-001..010, L2-ENGAGE-005, L2-MENTOR-008, L2-PLATFORM-011
@Component({
  selector: 'w-youth-home',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    Shell,
    RouterLink,
    DatePipe,
    DecimalPipe,
    Icon,
    Button,
    Alert,
    Card,
    CardBody,
    CardTitle,
    Scripture,
    ScriptureRef,
    Badge,
    EmptyState,
    EmptyStateIcon,
    EmptyStateTitle,
    EmptyStateText,
    Progress,
  ],
  templateUrl: './youth-home.html',
  styleUrl: './youth-home.css',
})
export class YouthHome {
  protected readonly auth = inject(AuthStore);
  private readonly learning = inject(LearningService);

  protected readonly firstName = computed(
    () => (this.auth.user()?.displayName ?? '').split(/\s+/)[0] || 'friend',
  );
  protected readonly home = signal<YouthHomeView | undefined>(undefined);
  protected readonly errorMessage = signal<string | null>(null);

  constructor() {
    this.learning.home().subscribe({
      next: (home) => this.home.set(home),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }
}
