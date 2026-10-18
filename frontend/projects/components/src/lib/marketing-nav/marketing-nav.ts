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
  templateUrl: './marketing-nav.html',
  styleUrl: './marketing-nav.css',
})
export class MarketingNav {}
