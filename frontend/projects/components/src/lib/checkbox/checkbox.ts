import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * Lets people turn one option on or off. Applied to a native `label` that wraps
 * a native `input[type=checkbox]` followed by a `ph-checkbox-box`, so forms, focus
 * and the accessible name stay native.
 */
@Component({
  selector: 'label[ph-checkbox]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-check' },
  templateUrl: './checkbox.html',
  styleUrl: './checkbox.css',
})
export class Checkbox {}
