import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The large figure of a stat. Also works outside a `ph-stat` in compact layouts. */
@Component({
  selector: 'div[ph-stat-value], span[ph-stat-value], strong[ph-stat-value]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-statcard__value' },
  templateUrl: './stat-value.html',
  styleUrl: './stat-value.css',
})
export class StatValue {}
