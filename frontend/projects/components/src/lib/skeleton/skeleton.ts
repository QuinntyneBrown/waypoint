import { ChangeDetectionStrategy, Component, ViewEncapsulation, input } from '@angular/core';

export type SkeletonVariant = 'text' | 'title' | 'circle' | 'block' | 'btn';

/**
 * A shimmering placeholder shaped like the content that is loading. Applied to a
 * `span`; it is decorative, so put the loading announcement on the container.
 */
@Component({
  selector: 'span[ph-skeleton]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'ph-skeleton',
    '[class.ph-skeleton--text]': "variant() === 'text'",
    '[class.ph-skeleton--title]': "variant() === 'title'",
    '[class.ph-skeleton--circle]': "variant() === 'circle'",
    '[class.ph-skeleton--block]': "variant() === 'block'",
    '[class.ph-skeleton--btn]': "variant() === 'btn'",
  },
  templateUrl: './skeleton.html',
  styleUrl: './skeleton.css',
})
export class Skeleton {
  readonly variant = input<SkeletonVariant | ''>('');
}
