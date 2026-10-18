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
  template: '<ng-content />',
  styles: `
    .w-pill {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 6px 14px;
      border-radius: var(--w-r-pill);
      border: var(--w-bw) solid var(--w-border-strong);
      font-size: 0.85rem;
      font-weight: 500;
      background: var(--w-white);
      cursor: pointer;
    }
    .w-pill.is-active {
      background: var(--w-ink);
      color: var(--w-white);
      border-color: var(--w-ink);
    }
  `,
})
export class Chip {
  readonly active = input(false, { transform: booleanAttribute });
}

/** Lays chips out in a wrapping row. */
@Component({
  selector: 'div[w-chip-row]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-pillrow' },
  template: '<ng-content />',
  styles: `
    .w-pillrow {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
    }
  `,
})
export class ChipRow {}
