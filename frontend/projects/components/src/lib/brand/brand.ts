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
  template: '<span class="w-brand__mark">W</span><span class="w-brand__name"><ng-content /></span>',
  styles: `
    .w-brand {
      display: inline-flex;
      align-items: center;
      gap: var(--w-s2);
      font-family: var(--w-font-head);
      font-weight: 700;
      letter-spacing: -0.03em;
      font-size: 1.05rem;
      color: var(--w-ink);
    }
    .w-brand:hover {
      text-decoration: none;
    }
    .w-brand__mark {
      width: 30px;
      height: 30px;
      border-radius: 7px;
      background: var(--w-lime);
      color: var(--w-on-lime);
      display: grid;
      place-items: center;
      font-weight: 700;
      font-size: 0.95rem;
      flex: none;
    }
    .w-brand--lg {
      gap: 10px;
      letter-spacing: 0;
      font-size: 1.2rem;
    }
    .w-brand--lg .w-brand__mark {
      width: 38px;
      height: 38px;
      font-size: 1.1rem;
    }
  `,
})
export class Brand {
  readonly size = input<'lg' | ''>('');
}
