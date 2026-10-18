import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * Lets people turn one option on or off. Applied to a native `label` that wraps
 * a native `input[type=checkbox]` followed by a `w-checkbox-box`, so forms, focus
 * and the accessible name stay native.
 */
@Component({
  selector: 'label[w-checkbox]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-check' },
  templateUrl: './checkbox.html',
  styleUrl: './checkbox.css',
})
export class Checkbox {}
