import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  booleanAttribute,
  input,
} from '@angular/core';

/** Scrolls a wide table sideways inside a bordered frame instead of widening the page. */
@Component({
  selector: 'div[w-table-wrap]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-table-wrap' },
  template: '<ng-content />',
  styles: `
    .w-table-wrap {
      width: 100%;
      overflow-x: auto;
      border: var(--w-bw) solid var(--w-border);
      border-radius: var(--w-r-card);
      background: var(--w-white);
    }
  `,
})
export class WTableWrap {}

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
  template: '<ng-content />',
  styles: `
    .w-table {
      width: 100%;
      border-collapse: collapse;
      font-size: 0.92rem;
    }
    .w-table th {
      text-align: left;
      font-family: var(--w-font-head);
      text-transform: uppercase;
      letter-spacing: 0.06em;
      font-size: 0.68rem;
      font-weight: 600;
      color: var(--w-muted);
      padding: 12px 16px;
      border-bottom: var(--w-bw) solid var(--w-border);
      white-space: nowrap;
      background: var(--w-g100);
    }
    .w-table td {
      padding: 13px 16px;
      border-bottom: var(--w-bw) solid var(--w-border);
      vertical-align: middle;
    }
    .w-table tbody tr:last-child td {
      border-bottom: 0;
    }
    .w-table tbody tr:hover {
      background: var(--w-g100);
    }
    @media (max-width: 760px) {
      .w-table--cards thead {
        display: none;
      }
      .w-table--cards,
      .w-table--cards tbody,
      .w-table--cards tr,
      .w-table--cards td {
        display: block;
        width: 100%;
      }
      .w-table--cards tr {
        border: var(--w-bw) solid var(--w-border);
        border-radius: var(--w-r-card);
        margin-bottom: var(--w-s3);
        padding: 6px 4px;
        background: var(--w-white);
      }
      .w-table--cards td {
        border: 0;
        display: flex;
        justify-content: space-between;
        gap: var(--w-s4);
        padding: 8px 14px;
        text-align: right;
        flex-wrap: wrap;
      }
      .w-table--cards td > * {
        min-width: 0;
      }
      .w-table--cards td::before {
        content: attr(data-label);
        font-family: var(--w-font-head);
        text-transform: uppercase;
        letter-spacing: 0.05em;
        font-size: 0.66rem;
        font-weight: 600;
        color: var(--w-muted);
        text-align: left;
      }
      .w-table--cards td:first-child {
        border-bottom: var(--w-bw) solid var(--w-border);
        margin-bottom: 4px;
      }
    }
  `,
})
export class WTable {
  readonly cards = input(false, { transform: booleanAttribute });
}

/**
 * A grid that compares people against steps, with a sticky first column. Applied
 * to a native `table`; set its `min-width` for the number of columns.
 */
@Component({
  selector: 'table[w-matrix]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-matrix' },
  template: '<ng-content />',
  styles: `
    .w-matrix {
      border-collapse: collapse;
      font-size: 0.82rem;
      min-width: 640px;
    }
    .w-matrix th,
    .w-matrix td {
      border: var(--w-bw) solid var(--w-border);
      padding: 8px 10px;
      text-align: center;
    }
    .w-matrix th:first-child,
    .w-matrix td:first-child {
      text-align: left;
      position: sticky;
      left: 0;
      background: var(--w-white);
      z-index: 1;
      font-weight: 600;
    }
    .w-matrix thead th {
      background: var(--w-g100);
      font-family: var(--w-font-head);
      text-transform: uppercase;
      font-size: 0.64rem;
      letter-spacing: 0.05em;
    }
  `,
})
export class WMatrix {}
