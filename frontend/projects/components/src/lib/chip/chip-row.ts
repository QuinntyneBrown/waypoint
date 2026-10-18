import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** Lays chips out in a wrapping row. */
@Component({
  selector: 'div[w-chip-row]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-pillrow' },
  templateUrl: './chip-row.html',
  styleUrl: './chip-row.css',
})
export class ChipRow {}
