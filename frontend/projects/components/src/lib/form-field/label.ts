import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** Names a control. Bind it with `for`/`id`. */
@Component({
  selector: 'label[ph-label], span[ph-label], legend[ph-label]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-label' },
  templateUrl: './label.html',
  styleUrl: './label.css',
})
export class Label {}
