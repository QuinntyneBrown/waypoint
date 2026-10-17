import { ChangeDetectionStrategy, Component, ViewEncapsulation, input } from '@angular/core';

export type WSkeletonVariant = 'text' | 'title' | 'circle' | 'block' | 'btn';

/**
 * A shimmering placeholder shaped like the content that is loading. Applied to a
 * `span`; it is decorative, so put the loading announcement on the container.
 */
@Component({
  selector: 'span[w-skeleton]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'w-skeleton',
    '[class.w-skeleton--text]': "variant() === 'text'",
    '[class.w-skeleton--title]': "variant() === 'title'",
    '[class.w-skeleton--circle]': "variant() === 'circle'",
    '[class.w-skeleton--block]': "variant() === 'block'",
    '[class.w-skeleton--btn]': "variant() === 'btn'",
  },
  template: '',
  styles: `
    .w-skeleton {
      background: linear-gradient(90deg, var(--w-g200) 25%, var(--w-g100) 37%, var(--w-g200) 63%);
      background-size: 400% 100%;
      animation: w-shimmer 1.4s ease infinite;
      border-radius: 6px;
      display: block;
    }
    .w-skeleton--text {
      height: 0.8em;
      margin: 0.35em 0;
      border-radius: 4px;
    }
    .w-skeleton--title {
      height: 1.4em;
      width: 50%;
      margin-bottom: 0.6em;
    }
    .w-skeleton--circle {
      border-radius: 50%;
    }
    .w-skeleton--block {
      height: 120px;
      border-radius: var(--w-r-card);
    }
    .w-skeleton--btn {
      height: 44px;
      width: 120px;
      border-radius: var(--w-r-pill);
    }
  `,
})
export class WSkeleton {
  readonly variant = input<WSkeletonVariant | ''>('');
}
