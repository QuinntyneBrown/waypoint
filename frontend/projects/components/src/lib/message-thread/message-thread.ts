import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * A conversation as a column of bubbles. Applied to a `div`; make it a live region
 * (`role="log"`, `aria-live="polite"`) so new messages are announced.
 */
@Component({
  selector: 'div[ph-message-thread]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-thread' },
  templateUrl: './message-thread.html',
  styleUrl: './message-thread.css',
})
export class MessageThread {}
