import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** One row of a list: a title with supporting detail, separated from its neighbours by a rule. */
@Component({
  selector: 'div[ph-list-item], li[ph-list-item]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-listitem' },
  templateUrl: './list-item.html',
  styleUrl: './list-item.css',
})
export class ListItem {}
