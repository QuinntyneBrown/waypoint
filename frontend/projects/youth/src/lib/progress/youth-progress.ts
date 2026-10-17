import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ApiError, LearningService, YouthProgress } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WShell } from '@waypoint/components';

type ProgressTab = 'overview' | 'quizzes' | 'attendance';

// Traces to: L2-LEARN-010
@Component({
  selector: 'w-youth-progress',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, DecimalPipe, DatePipe],
  templateUrl: './youth-progress.html',
  styles: `
    .progress-tabs {
      display: flex;
      gap: var(--w-s2);
      overflow-x: auto;
    }
    .progress-tab {
      min-height: 2.75rem;
      flex: 1 0 auto;
    }
    .progress-stat {
      min-height: 8rem;
    }
    .progress-stat__value {
      font-size: clamp(2rem, 8vw, 3.25rem);
      font-weight: 800;
      line-height: 1;
    }
  `,
})
export class YouthProgressPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(LearningService);
  protected readonly progress = signal<YouthProgress | undefined>(undefined);
  protected readonly activeTab = signal<ProgressTab>('overview');
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly tabs: readonly { id: ProgressTab; label: string }[] = [
    { id: 'overview', label: 'Overview' },
    { id: 'quizzes', label: 'Quizzes' },
    { id: 'attendance', label: 'Attendance' },
  ];

  constructor() {
    this.api.progress().subscribe({
      next: (progress) => this.progress.set(progress),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  protected selectTab(tab: ProgressTab): void {
    this.activeTab.set(tab);
  }

  protected tabKeydown(event: KeyboardEvent, index: number): void {
    if (!['ArrowLeft', 'ArrowRight', 'Home', 'End'].includes(event.key)) return;
    event.preventDefault();
    let next = index;
    if (event.key === 'ArrowRight') next = (index + 1) % this.tabs.length;
    if (event.key === 'ArrowLeft') next = (index - 1 + this.tabs.length) % this.tabs.length;
    if (event.key === 'Home') next = 0;
    if (event.key === 'End') next = this.tabs.length - 1;
    this.activeTab.set(this.tabs[next].id);
    const buttons = (
      event.currentTarget as HTMLElement
    ).parentElement?.querySelectorAll<HTMLButtonElement>('[role="tab"]');
    buttons?.[next]?.focus();
  }
}
