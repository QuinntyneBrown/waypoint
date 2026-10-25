import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** Red "required" marker inside a label. Decorative: the control has `required`. */
@Component({
  selector: 'span[ph-label-required]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-label__req' },
  templateUrl: './label-required.html',
  styleUrl: './label-required.css',
})
export class LabelRequired {}
