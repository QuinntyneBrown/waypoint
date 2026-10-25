import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The padded content area of a `ph-card`. */
@Component({
  selector: 'div[ph-card-body], form[ph-card-body], a[ph-card-body], span[ph-card-body]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-card__body' },
  templateUrl: './card-body.html',
  styleUrl: './card-body.css',
})
export class CardBody {}
