import { ChangeDetectionStrategy, Component, ViewEncapsulation, input } from '@angular/core';

/** A smaller line under the figure; `trend` colours it for a rise or a fall. */
@Component({
  selector: 'div[w-stat-delta]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'w-statcard__delta',
    '[class.is-up]': "trend() === 'up'",
    '[class.is-down]': "trend() === 'down'",
  },
  templateUrl: './stat-delta.html',
  styleUrl: './stat-delta.css',
})
export class StatDelta {
  readonly trend = input<'up' | 'down' | ''>('');
}
