import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The send time under a message. */
@Component({
  selector: 'time[ph-message-bubble-time]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-bubble__time' },
  templateUrl: './message-bubble-time.html',
  styleUrl: './message-bubble-time.css',
})
export class MessageBubbleTime {}
