import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The heading of a `ph-card`; use the heading level that fits the page outline. */
@Component({
  selector:
    'h1[ph-card-title], h2[ph-card-title], h3[ph-card-title], h4[ph-card-title], p[ph-card-title], a[ph-card-title]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-card__title' },
  templateUrl: './card-title.html',
  styleUrl: './card-title.css',
})
export class CardTitle {}
