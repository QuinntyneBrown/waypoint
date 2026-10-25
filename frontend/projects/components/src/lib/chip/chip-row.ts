import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** Lays chips out in a wrapping row. */
@Component({
  selector: 'div[ph-chip-row]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-pillrow' },
  templateUrl: './chip-row.html',
  styleUrl: './chip-row.css',
})
export class ChipRow {}
