import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The send time under a message. */
@Component({
  selector: 'time[w-message-bubble-time]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-bubble__time' },
  templateUrl: './message-bubble-time.html',
  styleUrl: './message-bubble-time.css',
})
export class MessageBubbleTime {}
