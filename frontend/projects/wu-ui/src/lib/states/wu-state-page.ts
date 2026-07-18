import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { WuIcon } from '../icon/wu-icon';

export type WuStateTone = 'warning' | 'error' | 'info' | 'success';

/**
 * Full-page system/error state (no app shell): centered card with a toned icon,
 * eyebrow, title, body, and projected action buttons. Mirrors the mock's
 * `.wu-authpage > .wu-empty` state layout.
 */
@Component({
  selector: 'wu-state-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuIcon],
  template: `<div class="wu-authpage">
    <div class="wu-empty" style="max-width: 460px">
      <div class="wu-empty__icon" [style.background]="iconBg()" [style.color]="iconColor()">
        <wu-icon [name]="icon()" iconClass="wu-state-page__glyph" />
      </div>
      @if (eyebrow()) {
        <div class="wu-eyebrow">{{ eyebrow() }}</div>
      }
      <h1 class="wu-empty__title" style="font-size: 1.6rem">{{ title() }}</h1>
      @if (text()) {
        <p class="wu-empty__text">{{ text() }}</p>
      }
      <div class="wu-cluster" style="justify-content: center">
        <ng-content />
      </div>
    </div>
  </div>`,
  styles: `
    .wu-state-page__glyph {
      width: 38px;
      height: 38px;
    }
  `,
})
export class WuStatePage {
  readonly icon = input('alert-triangle');
  readonly tone = input<WuStateTone>('warning');
  readonly eyebrow = input('');
  readonly title = input.required<string>();
  readonly text = input('');

  protected readonly iconBg = computed(() => `var(--wu-${this.tone()}-bg)`);
  protected readonly iconColor = computed(() => `var(--wu-${this.tone()})`);
}
