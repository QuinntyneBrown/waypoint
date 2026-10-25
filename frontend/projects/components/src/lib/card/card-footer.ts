import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The bottom row of a `ph-card`, separated by a rule; usually actions. */
@Component({
  selector: 'div[ph-card-footer]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-card__footer' },
  templateUrl: './card-footer.html',
  styleUrl: './card-footer.css',
})
export class CardFooter {}
