import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

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
  templateUrl: './tabs.html',
  styleUrl: './tabs.css',
})
export class Tabs {}
