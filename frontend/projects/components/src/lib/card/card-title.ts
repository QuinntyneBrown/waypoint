import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The heading of a `w-card`; use the heading level that fits the page outline. */
@Component({
  selector:
    'h1[w-card-title], h2[w-card-title], h3[w-card-title], h4[w-card-title], p[w-card-title], a[w-card-title]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-card__title' },
  templateUrl: './card-title.html',
  styleUrl: './card-title.css',
})
export class CardTitle {}
