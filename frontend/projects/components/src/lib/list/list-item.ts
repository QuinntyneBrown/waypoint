import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** One row of a list: a title with supporting detail, separated from its neighbours by a rule. */
@Component({
  selector: 'div[w-list-item], li[w-list-item]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-listitem' },
  templateUrl: './list-item.html',
  styleUrl: './list-item.css',
})
export class ListItem {}
