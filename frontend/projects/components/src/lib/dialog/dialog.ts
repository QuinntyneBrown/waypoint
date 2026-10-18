import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  inject,
  input,
  output,
} from '@angular/core';
import { AccessibleDialog } from './accessible-dialog';

/**
 * Interrupts the page to ask for a decision or short input. Render it only while
 * it is open (`@if`): it draws a scrim and a panel, traps focus, returns focus to
 * the opener when it is destroyed, and emits `dismiss` when people press Escape or
 * click the scrim. Name it with `aria-labelledby` (the `w-dialog-title` id) or
 * `aria-label`. Compose the panel from `w-dialog-header`, `w-dialog-body` and
 * `w-dialog-footer`.
 */
@Component({
  selector: 'div[w-dialog]',
  hostDirectives: [AccessibleDialog],
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'w-modal is-open',
    '[class.w-modal--lg]': "size() === 'lg'",
    '[class.w-modal--sm]': "size() === 'sm'",
  },
  templateUrl: './dialog.html',
  styleUrl: './dialog.css',
})
export class Dialog {
  readonly size = input<'sm' | 'lg' | ''>('');
  /** Escape was pressed or the scrim was clicked. */
  readonly dismiss = output<void>();

  constructor() {
    inject(AccessibleDialog).escape.subscribe(() => this.dismiss.emit());
  }
}
