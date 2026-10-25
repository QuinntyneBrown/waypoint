import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** Scrolls a wide table sideways inside a bordered frame instead of widening the page. */
@Component({
  selector: 'div[ph-table-wrap]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-table-wrap' },
  templateUrl: './table-wrap.html',
  styleUrl: './table-wrap.css',
})
export class TableWrap {}
