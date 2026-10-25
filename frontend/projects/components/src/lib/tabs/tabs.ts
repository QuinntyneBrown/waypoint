import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * Switches between related views or pages. Applied to a `div` (`role="tablist"`
 * with `ph-tab` buttons) or to a `nav` (with `ph-tab` links); give it an accessible
 * name.
 */
@Component({
  selector: 'div[ph-tabs], nav[ph-tabs]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-tabs' },
  templateUrl: './tabs.html',
  styleUrl: './tabs.css',
})
export class Tabs {}
