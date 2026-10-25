import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The top row of a `ph-card`: usually its title and an optional action. */
@Component({
  selector: 'div[ph-card-header]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-card__header' },
  templateUrl: './card-header.html',
  styleUrl: './card-header.css',
})
export class CardHeader {}
