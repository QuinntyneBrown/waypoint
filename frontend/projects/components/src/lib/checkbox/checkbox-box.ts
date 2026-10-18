import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The visible square of a `w-checkbox`; put a check icon inside or leave it empty. */
@Component({
  selector: 'span[w-checkbox-box]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-check__box' },
  templateUrl: './checkbox-box.html',
  styleUrl: './checkbox-box.css',
})
export class CheckboxBox {}
