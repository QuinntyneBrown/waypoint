import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  booleanAttribute,
  input,
} from '@angular/core';

/** A vertical line of steps in order. Applied to a `div` holding `w-timeline-item` steps. */
@Component({
  selector: 'div[w-timeline]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-timeline' },
  template: '<ng-content />',
  styles: `
    .w-timeline {
      position: relative;
      padding-left: 26px;
    }
    .w-timeline::before {
      content: '';
      position: absolute;
      left: 7px;
      top: 6px;
      bottom: 6px;
      width: 2px;
      background: var(--w-border);
    }
  `,
})
export class WTimeline {}

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
  template: '<ng-content />',
  styles: `
    .w-timeline__item {
      position: relative;
      padding-bottom: var(--w-s5);
    }
    .w-timeline__item::before {
      content: '';
      position: absolute;
      left: -26px;
      top: 3px;
      width: 16px;
      height: 16px;
      border-radius: 50%;
      background: var(--w-white);
      border: 2px solid var(--w-ink);
    }
    .w-timeline__item.is-done::before {
      background: var(--w-lime);
      border-color: var(--w-lime);
    }
    .w-timeline__item.is-current::before {
      background: var(--w-ink);
    }
  `,
})
export class WTimelineItem {
  readonly done = input(false, { transform: booleanAttribute });
  readonly current = input(false, { transform: booleanAttribute });
}
