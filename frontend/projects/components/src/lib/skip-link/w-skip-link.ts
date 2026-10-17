import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * Lets keyboard users jump past repeated navigation. Applied to a native `a` whose
 * `href` points at the main content (`#main`). It is off-screen until it is focused,
 * so make it the first focusable element on the page.
 */
@Component({
  selector: 'a[w-skip-link]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-skip-link' },
  template: '<ng-content />',
  styles: `
    .w-skip-link {
      position: absolute;
      left: var(--w-s4);
      top: -60px;
      z-index: 100;
      padding: var(--w-s2) var(--w-s4);
      background: var(--w-ink);
      color: var(--w-white);
      border-radius: var(--w-r-input);
      transition: top 0.15s ease;
    }
    .w-skip-link:focus {
      top: var(--w-s3);
    }
  `,
})
export class WSkipLink {}
