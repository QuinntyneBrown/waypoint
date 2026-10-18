import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The first line of a `w-list-item`. */
@Component({
  selector: 'div[w-list-item-title]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-listitem__title' },
  templateUrl: './list-item-title.html',
  styleUrl: './list-item-title.css',
})
export class ListItemTitle {}
