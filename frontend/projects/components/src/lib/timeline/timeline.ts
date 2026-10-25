import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** A vertical line of steps in order. Applied to a `div` holding `ph-timeline-item` steps. */
@Component({
  selector: 'div[ph-timeline]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-timeline' },
  templateUrl: './timeline.html',
  styleUrl: './timeline.css',
})
export class Timeline {}
