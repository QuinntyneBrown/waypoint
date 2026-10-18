import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The padded content area of a `w-card`. */
@Component({
  selector: 'div[w-card-body], form[w-card-body], a[w-card-body], span[w-card-body]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-card__body' },
  templateUrl: './card-body.html',
  styleUrl: './card-body.css',
})
export class CardBody {}
