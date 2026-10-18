import { ChangeDetectionStrategy, Component, ViewEncapsulation, input } from '@angular/core';

/**
 * The Waypoint mark and name. Applied to an `a` (links home) or a `div` (plain).
 * The component draws the mark; put the name, "Waypoint", inside as its content.
 * Use the `lg` size on sign-in cards.
 */
@Component({
  selector: 'a[w-brand], div[w-brand]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-brand', '[class.w-brand--lg]': "size() === 'lg'" },
  templateUrl: './brand.html',
  styleUrl: './brand.css',
})
export class Brand {
  readonly size = input<'lg' | ''>('');
}
