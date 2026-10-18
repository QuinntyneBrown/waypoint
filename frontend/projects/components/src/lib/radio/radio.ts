import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * Lets people pick one option from a set. Applied to a native `label` that wraps
 * a native `input[type=radio]` followed by a `w-radio-box`, so the group, arrow-key
 * movement and the accessible name stay native.
 */
@Component({
  selector: 'label[w-radio]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-radio' },
  templateUrl: './radio.html',
  styleUrl: './radio.css',
})
export class Radio {}
