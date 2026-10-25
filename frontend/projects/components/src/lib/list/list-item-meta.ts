import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The quieter second line of a `ph-list-item`. */
@Component({
  selector: 'div[ph-list-item-meta]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-listitem__meta' },
  templateUrl: './list-item-meta.html',
  styleUrl: './list-item-meta.css',
})
export class ListItemMeta {}
