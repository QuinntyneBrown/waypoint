import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The large figure of a stat. Also works outside a `w-stat` in compact layouts. */
@Component({
  selector: 'div[w-stat-value], span[w-stat-value], strong[w-stat-value]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-statcard__value' },
  templateUrl: './stat-value.html',
  styleUrl: './stat-value.css',
})
export class StatValue {}
