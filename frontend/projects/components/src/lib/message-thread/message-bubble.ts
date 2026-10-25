import { ChangeDetectionStrategy, Component, ViewEncapsulation, input } from '@angular/core';

/**
 * One message. `direction` says who sent it: `out` is the person reading (right,
 * accent colour), `in` is someone else (left, grey). Name the sender for screen
 * readers inside the bubble, since colour and side are not enough.
 */
@Component({
  selector: 'article[ph-message-bubble]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'ph-bubble',
    '[class.ph-bubble--in]': "direction() === 'in'",
    '[class.ph-bubble--out]': "direction() === 'out'",
  },
  templateUrl: './message-bubble.html',
  styleUrl: './message-bubble.css',
})
export class MessageBubble {
  readonly direction = input<'in' | 'out'>('in');
}
