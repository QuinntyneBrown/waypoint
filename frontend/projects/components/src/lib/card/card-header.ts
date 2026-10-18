import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The top row of a `w-card`: usually its title and an optional action. */
@Component({
  selector: 'div[w-card-header]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-card__header' },
  templateUrl: './card-header.html',
  styleUrl: './card-header.css',
})
export class CardHeader {}
