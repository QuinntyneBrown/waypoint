import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The text column of a `w-list-item`; it takes the remaining width. */
@Component({
  selector: 'div[w-list-item-body]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-listitem__body' },
  templateUrl: './list-item-body.html',
  styleUrl: './list-item-body.css',
})
export class ListItemBody {}
