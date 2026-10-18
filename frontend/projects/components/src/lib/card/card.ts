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
  template: '<ng-content />',
  styles: `
    .w-card {
      background: var(--w-white);
      border: var(--w-bw) solid var(--w-border);
      border-radius: var(--w-r-card);
      overflow: hidden;
    }
    .w-card--greige {
      background: var(--w-surface);
      border-color: transparent;
    }
    .w-card--ink {
      background: var(--w-ink);
      color: var(--w-white);
      border-color: transparent;
    }
    .w-card--lime {
      background: var(--w-lime);
      color: var(--w-on-lime);
      border-color: transparent;
    }
    /* Primary buttons on a lime card would be lime-on-lime; invert them. */
    .w-card--lime .w-btn--primary {
      background: var(--w-ink);
      color: var(--w-white);
    }
    .w-card--lime .w-btn--primary:hover {
      background: var(--w-ink-80);
    }
    /* Headings default to ink; on an ink card they must follow the card colour. */
    .w-card--ink :is(h1, h2, h3, h4, h5, h6) {
      color: inherit;
    }
    .w-card--ink .w-eyebrow {
      color: color-mix(in srgb, var(--w-white) 75%, transparent);
    }
    .w-card--flat {
      border: 0;
      background: var(--w-surface);
    }
    .w-card--link {
      cursor: pointer;
      transition:
        border-color 0.15s,
        transform 0.12s;
    }
    .w-card--link:hover {
      border-color: var(--w-ink);
    }
    .w-card--ink .w-card__footer,
    .w-card--lime .w-card__footer {
      border-color: color-mix(in srgb, var(--w-white) 18%, transparent);
    }
    .w-card--lime .w-card__footer {
      border-color: color-mix(in srgb, var(--w-on-lime) 18%, transparent);
    }
  `,
})
export class Card {
  readonly variant = input<CardVariant | ''>('');
  readonly link = input(false, { transform: booleanAttribute });
}

/** The top row of a `w-card`: usually its title and an optional action. */
@Component({
  selector: 'div[w-card-header]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-card__header' },
  template: '<ng-content />',
  styles: `
    .w-card__header {
      padding: var(--w-s5) var(--w-s5) 0;
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: var(--w-s3);
    }
  `,
})
export class CardHeader {}

/** The heading of a `w-card`; use the heading level that fits the page outline. */
@Component({
  selector:
    'h1[w-card-title], h2[w-card-title], h3[w-card-title], h4[w-card-title], p[w-card-title], a[w-card-title]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-card__title' },
  template: '<ng-content />',
  styles: `
    .w-card__title {
      font-size: 1.05rem;
      margin: 0;
    }
  `,
})
export class CardTitle {}

/** The padded content area of a `w-card`. */
@Component({
  selector: 'div[w-card-body], form[w-card-body], a[w-card-body], span[w-card-body]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-card__body' },
  template: '<ng-content />',
  styles: `
    .w-card__body {
      padding: var(--w-s5);
    }
  `,
})
export class CardBody {}

/** The bottom row of a `w-card`, separated by a rule; usually actions. */
@Component({
  selector: 'div[w-card-footer]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-card__footer' },
  template: '<ng-content />',
  styles: `
    .w-card__footer {
      padding: var(--w-s4) var(--w-s5);
      border-top: var(--w-bw) solid var(--w-border);
      display: flex;
      align-items: center;
      gap: var(--w-s3);
      flex-wrap: wrap;
    }
  `,
})
export class CardFooter {}
