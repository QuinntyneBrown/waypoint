import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The first line of a `ph-list-item`. */
@Component({
  selector: 'div[ph-list-item-title]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-listitem__title' },
  templateUrl: './list-item-title.html',
  styleUrl: './list-item-title.css',
})
export class ListItemTitle {}
