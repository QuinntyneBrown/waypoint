import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The visible square of a `ph-checkbox`; put a check icon inside or leave it empty. */
@Component({
  selector: 'span[ph-checkbox-box]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-check__box' },
  templateUrl: './checkbox-box.html',
  styleUrl: './checkbox-box.css',
})
export class CheckboxBox {}
