import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The bottom row of a `w-card`, separated by a rule; usually actions. */
@Component({
  selector: 'div[w-card-footer]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-card__footer' },
  templateUrl: './card-footer.html',
  styleUrl: './card-footer.css',
})
export class CardFooter {}
