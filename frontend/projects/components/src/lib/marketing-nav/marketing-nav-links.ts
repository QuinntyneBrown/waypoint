import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The page links in a `w-marketing-nav`; hidden on narrow screens. Give it an `aria-label`. */
@Component({
  selector: 'nav[w-marketing-nav-links]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-marketing-nav__links' },
  templateUrl: './marketing-nav-links.html',
  styleUrl: './marketing-nav-links.css',
})
export class MarketingNavLinks {}
