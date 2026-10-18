import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The caption above a stat's figure. Also works outside a `w-stat` in compact layouts. */
@Component({
  selector: 'div[w-stat-label], span[w-stat-label]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-statcard__label' },
  templateUrl: './stat-label.html',
  styleUrl: './stat-label.css',
})
export class StatLabel {}
