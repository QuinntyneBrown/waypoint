import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** A vertical line of steps in order. Applied to a `div` holding `w-timeline-item` steps. */
@Component({
  selector: 'div[w-timeline]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-timeline' },
  templateUrl: './timeline.html',
  styleUrl: './timeline.css',
})
export class Timeline {}
