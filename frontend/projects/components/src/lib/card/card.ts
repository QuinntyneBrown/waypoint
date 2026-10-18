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
 * `w-card-header`, `w-card-title`, `w-card-body` and `w-card-footer`. Use the
 * `link` input on an `a` that makes the whole card a link; it combines with any variant.
 */
@Component({
  selector:
    'div[w-card], section[w-card], article[w-card], aside[w-card], a[w-card], form[w-card], fieldset[w-card], label[w-card], li[w-card], span[w-card], p[w-card]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'w-card',
    '[class.w-card--greige]': "variant() === 'greige'",
    '[class.w-card--ink]': "variant() === 'ink'",
    '[class.w-card--lime]': "variant() === 'lime'",
    '[class.w-card--flat]': "variant() === 'flat'",
    '[class.w-card--link]': 'link()',
  },
  templateUrl: './card.html',
  styleUrl: './card.css',
})
export class Card {
  readonly variant = input<CardVariant | ''>('');
  readonly link = input(false, { transform: booleanAttribute });
}
