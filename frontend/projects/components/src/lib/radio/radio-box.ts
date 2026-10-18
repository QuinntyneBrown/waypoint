import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The visible circle of a `w-radio`; leave it empty. */
@Component({
  selector: 'span[w-radio-box]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-radio__box' },
  templateUrl: './radio-box.html',
  styleUrl: './radio-box.css',
})
export class RadioBox {}
