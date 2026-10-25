import { ChangeDetectionStrategy, Component, ViewEncapsulation, input } from '@angular/core';

/**
 * Shows a percentage as a ring with its figure in the middle. Applied to a `div`
 * that holds a `ph-progress-ring-label`.
 */
@Component({
  selector: 'div[ph-progress-ring]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'ph-ring',
    '[class.ph-ring--lg]': "size() === 'lg'",
    '[style.--val]': 'value()',
  },
  templateUrl: './progress-ring.html',
  styleUrl: './progress-ring.css',
})
export class ProgressRing {
  /** Percent complete, 0 to 100. */
  readonly value = input.required<number>();
  readonly size = input<'lg' | ''>('');
}
