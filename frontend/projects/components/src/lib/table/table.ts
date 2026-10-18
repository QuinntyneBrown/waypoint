import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  booleanAttribute,
  input,
} from '@angular/core';

/**
 * Lists records in rows and columns. Applied to a native `table`, so headers,
 * captions and sorting semantics stay native; give it an accessible name. With
 * `cards`, rows become stacked cards on small screens and each `td` shows its
 * `data-label` as a caption.
 */
@Component({
  selector: 'table[w-table]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-table', '[class.w-table--cards]': 'cards()' },
  templateUrl: './table.html',
  styleUrl: './table.css',
})
export class Table {
  readonly cards = input(false, { transform: booleanAttribute });
}
