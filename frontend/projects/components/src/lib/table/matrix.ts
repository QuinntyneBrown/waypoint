import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * A grid that compares people against steps, with a sticky first column. Applied
 * to a native `table`; set its `min-width` for the number of columns.
 */
@Component({
  selector: 'table[w-matrix]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-matrix' },
  templateUrl: './matrix.html',
  styleUrl: './matrix.css',
})
export class Matrix {}
