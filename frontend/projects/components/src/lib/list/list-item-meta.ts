import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The quieter second line of a `w-list-item`. */
@Component({
  selector: 'div[w-list-item-meta]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-listitem__meta' },
  templateUrl: './list-item-meta.html',
  styleUrl: './list-item-meta.css',
})
export class ListItemMeta {}
