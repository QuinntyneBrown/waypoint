import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The text column of a `ph-list-item`; it takes the remaining width. */
@Component({
  selector: 'div[ph-list-item-body]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-listitem__body' },
  templateUrl: './list-item-body.html',
  styleUrl: './list-item-body.css',
})
export class ListItemBody {}
