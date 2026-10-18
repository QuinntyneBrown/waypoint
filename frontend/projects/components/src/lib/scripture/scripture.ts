import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** Sets a passage or a reflection prompt apart in a serif quote block. */
@Component({
  selector: 'div[w-scripture], p[w-scripture]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-scripture' },
  templateUrl: './scripture.html',
  styleUrl: './scripture.css',
})
export class Scripture {}
