import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  booleanAttribute,
  input,
} from '@angular/core';

/**
 * Chooses one value from a few, shown side by side. Applied to a `div` that holds
 * `w-button-group-item` buttons; give it `role="group"` and an accessible name.
 */
@Component({
  selector: 'div[w-button-group]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-segment' },
  template: '<ng-content />',
  styles: `
    .w-segment {
      display: inline-flex;
      background: var(--w-g200);
      border-radius: var(--w-r-pill);
      padding: 3px;
    }
  `,
})
export class ButtonGroup {}

/** One choice in a `w-button-group`. `active` marks the chosen one and sets `aria-pressed`. */
@Component({
  selector: 'button[w-button-group-item]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'w-segment__item',
    '[class.is-active]': 'active()',
    '[attr.aria-pressed]': 'active()',
  },
  template: '<ng-content />',
  styles: `
    .w-segment__item {
      appearance: none;
      border: 0;
      background: transparent;
      cursor: pointer;
      padding: 7px 16px;
      border-radius: var(--w-r-pill);
      font-family: var(--w-font-head);
      font-weight: 500;
      font-size: 0.85rem;
      color: var(--w-ink-60);
    }
    .w-segment__item.is-active {
      background: var(--w-white);
      color: var(--w-ink);
      box-shadow: var(--w-shadow-sm);
    }
  `,
})
export class ButtonGroupItem {
  readonly active = input(false, { transform: booleanAttribute });
}
