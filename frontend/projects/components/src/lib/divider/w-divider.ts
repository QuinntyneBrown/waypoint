import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** A horizontal rule that separates groups of content within a page or a card. */
@Component({
  selector: 'div[w-divider]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-divider' },
  template: '',
  styles: `
    .w-divider {
      border-top: var(--w-bw) solid var(--w-border);
      margin-block: var(--w-s5);
    }
  `,
})
export class WDivider {}
