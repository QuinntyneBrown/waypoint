import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { EmptyState } from '../empty-state/empty-state';
import { EmptyStateIcon } from '../empty-state/empty-state-icon';
import { EmptyStateText } from '../empty-state/empty-state-text';
import { EmptyStateTitle } from '../empty-state/empty-state-title';
import { Icon } from '../icon/icon';

export type StateTone = 'warning' | 'error' | 'info' | 'success';

/**
 * Full-page system/error state (no app shell): centered card with a toned icon,
 * eyebrow, title, body, and projected action buttons. Mirrors the mock's
 * `.ph-authpage > .ph-empty` state layout.
 */
@Component({
  selector: 'ph-state-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Icon, EmptyState, EmptyStateIcon, EmptyStateTitle, EmptyStateText],
  templateUrl: './state-page.html',
  styleUrl: './state-page.css',
})
export class StatePage {
  readonly icon = input('alert-triangle');
  readonly tone = input<StateTone>('warning');
  readonly eyebrow = input('');
  readonly title = input.required<string>();
  readonly text = input('');

  protected readonly iconBg = computed(() => `var(--w-${this.tone()}-bg)`);
  protected readonly iconColor = computed(() => `var(--w-${this.tone()})`);
}
