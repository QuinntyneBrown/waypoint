import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * Chooses one value from a few, shown side by side. Applied to a `div` that holds
 * `ph-button-group-item` buttons; give it `role="group"` and an accessible name.
 */
@Component({
  selector: 'div[ph-button-group]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-segment' },
  templateUrl: './button-group.html',
  styleUrl: './button-group.css',
})
export class ButtonGroup {}
