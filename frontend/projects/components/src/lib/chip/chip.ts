import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  booleanAttribute,
  input,
} from '@angular/core';

/**
 * A small toggle that filters or selects. Applied to a native `button`; `active`
 * marks it selected and sets `aria-pressed`.
 */
@Component({
  selector: 'button[w-chip]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'w-pill',
    '[class.is-active]': 'active()',
    '[attr.aria-pressed]': 'active()',
  },
  templateUrl: './chip.html',
  styleUrl: './chip.css',
})
export class Chip {
  readonly active = input(false, { transform: booleanAttribute });
}
