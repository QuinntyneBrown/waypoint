import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  booleanAttribute,
  input,
} from '@angular/core';

/** One step on a `w-timeline`; `done` fills its marker and `current` marks where people are. */
@Component({
  selector: 'article[w-timeline-item], div[w-timeline-item]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'w-timeline__item',
    '[class.is-done]': 'done()',
    '[class.is-current]': 'current()',
  },
  templateUrl: './timeline-item.html',
  styleUrl: './timeline-item.css',
})
export class TimelineItem {
  readonly done = input(false, { transform: booleanAttribute });
  readonly current = input(false, { transform: booleanAttribute });
}
