import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * Turns a setting on or off straight away. Applied to a native `label` that wraps
 * a native `input[type=checkbox]` followed by a `ph-switch-track`, so forms, focus
 * and the accessible name stay native.
 */
@Component({
  selector: 'label[ph-switch]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-switch' },
  templateUrl: './switch.html',
  styleUrl: './switch.css',
})
export class Switch {}
