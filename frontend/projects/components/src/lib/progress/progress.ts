import { ChangeDetectionStrategy, Component, ViewEncapsulation, input } from '@angular/core';

/**
 * Shows how far along something is, as a bar. Applied to a `div`. Give it a
 * `label` and it is a `progressbar` with its value announced; leave `label` out
 * when the number is already printed next to it and add `aria-hidden="true"`.
 */
@Component({
  selector: 'div[ph-progress]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'ph-progress',
    '[attr.role]': "label() ? 'progressbar' : null",
    '[attr.aria-label]': 'label() || null',
    '[attr.aria-valuemin]': 'label() ? 0 : null',
    '[attr.aria-valuemax]': 'label() ? 100 : null',
    '[attr.aria-valuenow]': 'label() ? value() : null',
    '[style.--val]': "value() + '%'",
  },
  templateUrl: './progress.html',
  styleUrl: './progress.css',
})
export class Progress {
  /** Percent complete, 0 to 100. */
  readonly value = input.required<number>();
  /** Accessible name; makes the bar a `progressbar`. */
  readonly label = input('');
}
