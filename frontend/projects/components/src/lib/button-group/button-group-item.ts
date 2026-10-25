import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  booleanAttribute,
  input,
} from '@angular/core';

/** One choice in a `ph-button-group`. `active` marks the chosen one and sets `aria-pressed`. */
@Component({
  selector: 'button[ph-button-group-item]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'ph-segment__item',
    '[class.is-active]': 'active()',
    '[attr.aria-pressed]': 'active()',
  },
  templateUrl: './button-group-item.html',
  styleUrl: './button-group-item.css',
})
export class ButtonGroupItem {
  readonly active = input(false, { transform: booleanAttribute });
}
