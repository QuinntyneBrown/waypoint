import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import {
  EmptyState,
  EmptyStateIcon,
  EmptyStateText,
  EmptyStateTitle,
} from '../empty-state/empty-state';
import { Icon } from '../icon/icon';

export type StateTone = 'warning' | 'error' | 'info' | 'success';

/**
 * Full-page system/error state (no app shell): centered card with a toned icon,
 * eyebrow, title, body, and projected action buttons. Mirrors the mock's
 * `.w-authpage > .w-empty` state layout.
 */
@Component({
  selector: 'w-state-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Icon, EmptyState, EmptyStateIcon, EmptyStateTitle, EmptyStateText],
  template: `<div class="w-authpage">
    <div w-empty-state style="max-width: 460px">
      <div w-empty-state-icon [style.background]="iconBg()" [style.color]="iconColor()">
        <w-icon [name]="icon()" iconClass="w-state-page__glyph" />
      </div>
      @if (eyebrow()) {
        <div class="w-eyebrow">{{ eyebrow() }}</div>
      }
      <h1 w-empty-state-title style="font-size: 1.6rem">{{ title() }}</h1>
      @if (text()) {
        <p w-empty-state-text>{{ text() }}</p>
      }
      <div class="w-cluster" style="justify-content: center">
        <ng-content />
      </div>
    </div>
  </div>`,
  styles: `
    .w-state-page__glyph {
      width: 38px;
      height: 38px;
    }
  `,
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
