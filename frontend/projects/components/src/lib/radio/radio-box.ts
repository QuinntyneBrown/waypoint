import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The visible circle of a `ph-radio`; leave it empty. */
@Component({
  selector: 'span[ph-radio-box]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-radio__box' },
  templateUrl: './radio-box.html',
  styleUrl: './radio-box.css',
})
export class RadioBox {}
