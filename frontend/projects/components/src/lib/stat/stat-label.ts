import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The caption above a stat's figure. Also works outside a `ph-stat` in compact layouts. */
@Component({
  selector: 'div[ph-stat-label], span[ph-stat-label]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-statcard__label' },
  templateUrl: './stat-label.html',
  styleUrl: './stat-label.css',
})
export class StatLabel {}
