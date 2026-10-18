import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * The sticky bar at the top of the public site. Applied to a native `header`; put a
 * `w-brand`, a `w-marketing-nav-links` navigation and any actions inside, and a
 * `w-spacer` before the actions to push them to the right.
 */
@Component({
  selector: 'header[w-marketing-nav]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-marketing-nav' },
  template: '<div class="w-container w-marketing-nav__inner"><ng-content /></div>',
  styles: `
    .w-marketing-nav {
      position: sticky;
      top: 0;
      z-index: 50;
      background: var(--w-white);
      border-bottom: var(--w-bw) solid var(--w-border);
    }
    .w-marketing-nav__inner {
      display: flex;
      align-items: center;
      gap: var(--w-s5);
      height: 64px;
    }
  `,
})
export class MarketingNav {}

/** The page links in a `w-marketing-nav`; hidden on narrow screens. Give it an `aria-label`. */
@Component({
  selector: 'nav[w-marketing-nav-links]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-marketing-nav__links' },
  template: '<ng-content />',
  styles: `
    .w-marketing-nav__links {
      display: none;
      gap: var(--w-s5);
      margin-left: var(--w-s5);
    }
    @media (min-width: 820px) {
      .w-marketing-nav__links {
        display: flex;
      }
    }
  `,
})
export class MarketingNavLinks {}
