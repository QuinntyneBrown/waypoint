import { ChangeDetectionStrategy, Component, ViewEncapsulation, input } from '@angular/core';

/**
 * A conversation as a column of bubbles. Applied to a `div`; make it a live region
 * (`role="log"`, `aria-live="polite"`) so new messages are announced.
 */
@Component({
  selector: 'div[w-message-thread]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-thread' },
  template: '<ng-content />',
  styles: `
    .w-thread {
      display: flex;
      flex-direction: column;
      gap: var(--w-s2);
    }
  `,
})
export class WMessageThread {}

/**
 * One message. `direction` says who sent it: `out` is the person reading (right,
 * accent colour), `in` is someone else (left, grey). Name the sender for screen
 * readers inside the bubble, since colour and side are not enough.
 */
@Component({
  selector: 'article[w-message-bubble]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'w-bubble',
    '[class.w-bubble--in]': "direction() === 'in'",
    '[class.w-bubble--out]': "direction() === 'out'",
  },
  template: '<ng-content />',
  styles: `
    .w-bubble {
      max-width: min(75%, 32rem);
      padding: var(--w-s3) var(--w-s4);
      border-radius: var(--w-r-lg);
      overflow-wrap: anywhere;
    }
    .w-bubble--in {
      align-self: flex-start;
      background: var(--w-g200);
      border-bottom-left-radius: var(--w-r-sm);
    }
    .w-bubble--out {
      align-self: flex-end;
      background: var(--color-accent);
      color: var(--color-fg-on-accent);
      border-bottom-right-radius: var(--w-r-sm);
    }
  `,
})
export class WMessageBubble {
  readonly direction = input<'in' | 'out'>('in');
}

/** The send time under a message. */
@Component({
  selector: 'time[w-message-bubble-time]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-bubble__time' },
  template: '<ng-content />',
  styles: `
    .w-bubble__time {
      display: block;
      font-size: 0.75rem;
      opacity: 0.8;
      margin-top: var(--w-s1);
    }
  `,
})
export class WMessageBubbleTime {}
