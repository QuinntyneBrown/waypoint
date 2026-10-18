import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * Chooses one value from a few, shown side by side. Applied to a `div` that holds
 * `w-button-group-item` buttons; give it `role="group"` and an accessible name.
 */
@Component({
  selector: 'div[w-button-group]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-segment' },
  templateUrl: './button-group.html',
  styleUrl: './button-group.css',
})
export class ButtonGroup {}
