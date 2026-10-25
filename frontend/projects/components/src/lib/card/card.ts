import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  booleanAttribute,
  input,
} from '@angular/core';

export type CardVariant = 'greige' | 'ink' | 'lime' | 'flat';

/**
 * Groups related content on a bordered surface. Applied to the native element
 * that fits the content (`section`, `article`, `div`, `a`, `form`, ...), so
 * landmarks, links and forms keep their semantics. Compose it from
 * `ph-card-header`, `ph-card-title`, `ph-card-body` and `ph-card-footer`. Use the
 * `link` input on an `a` that makes the whole card a link; it combines with any variant.
 */
@Component({
  selector:
    'div[ph-card], section[ph-card], article[ph-card], aside[ph-card], a[ph-card], form[ph-card], fieldset[ph-card], label[ph-card], li[ph-card], span[ph-card], p[ph-card]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'ph-card',
    '[class.ph-card--greige]': "variant() === 'greige'",
    '[class.ph-card--ink]': "variant() === 'ink'",
    '[class.ph-card--lime]': "variant() === 'lime'",
    '[class.ph-card--flat]': "variant() === 'flat'",
    '[class.ph-card--link]': 'link()',
  },
  templateUrl: './card.html',
  styleUrl: './card.css',
})
export class Card {
  readonly variant = input<CardVariant | ''>('');
  readonly link = input(false, { transform: booleanAttribute });
}
