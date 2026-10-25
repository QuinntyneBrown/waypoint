import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * Lets people pick one option from a set. Applied to a native `label` that wraps
 * a native `input[type=radio]` followed by a `ph-radio-box`, so the group, arrow-key
 * movement and the accessible name stay native.
 */
@Component({
  selector: 'label[ph-radio]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-radio' },
  templateUrl: './radio.html',
  styleUrl: './radio.css',
})
export class Radio {}
