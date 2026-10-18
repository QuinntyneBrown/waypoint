import { ChangeDetectionStrategy, Component, ViewEncapsulation, input } from '@angular/core';

export type SkeletonVariant = 'text' | 'title' | 'circle' | 'block' | 'btn';

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
  templateUrl: './skeleton.html',
  styleUrl: './skeleton.css',
})
export class Skeleton {
  readonly variant = input<SkeletonVariant | ''>('');
}
