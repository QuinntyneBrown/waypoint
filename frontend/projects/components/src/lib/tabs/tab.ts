import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  booleanAttribute,
  input,
} from '@angular/core';

/**
 * One tab in `w-tabs`. Applied to a `button` (`role="tab"`, with `aria-selected`
 * and `aria-controls` from the page) or to an `a` for navigation. `active` marks
 * the current one.
 */
@Component({
  selector: 'button[w-tab], a[w-tab]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-tab', '[class.is-active]': 'active()' },
  templateUrl: './tab.html',
  styleUrl: './tab.css',
})
export class Tab {
  readonly active = input(false, { transform: booleanAttribute });
}
