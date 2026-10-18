import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** Names a control. Bind it with `for`/`id`. */
@Component({
  selector: 'label[w-label], span[w-label], legend[w-label]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-label' },
  templateUrl: './label.html',
  styleUrl: './label.css',
})
export class Label {}
