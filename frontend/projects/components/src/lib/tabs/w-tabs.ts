import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  booleanAttribute,
  input,
} from '@angular/core';

/**
 * Switches between related views or pages. Applied to a `div` (`role="tablist"`
 * with `w-tab` buttons) or to a `nav` (with `w-tab` links); give it an accessible
 * name.
 */
@Component({
  selector: 'div[w-tabs], nav[w-tabs]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-tabs' },
  template: '<ng-content />',
  styles: `
    .w-tabs {
      display: flex;
      gap: 2px;
      border-bottom: var(--w-bw) solid var(--w-border);
      margin-bottom: var(--w-s5);
      overflow-x: auto;
    }
  `,
})
export class WTabs {}

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
  template: '<ng-content />',
  styles: `
    .w-tab {
      appearance: none;
      border: 0;
      background: transparent;
      cursor: pointer;
      font-family: var(--w-font-head);
      font-weight: 500;
      font-size: 0.92rem;
      color: var(--w-muted);
      padding: 12px 14px;
      border-bottom: 2px solid transparent;
      white-space: nowrap;
      margin-bottom: -1.5px;
    }
    .w-tab:hover {
      color: var(--w-ink);
    }
    .w-tab.is-active {
      color: var(--w-ink);
      border-bottom-color: var(--w-ink);
    }
    .w-tab .w-badge {
      margin-left: 6px;
    }
  `,
})
export class WTab {
  readonly active = input(false, { transform: booleanAttribute });
}
