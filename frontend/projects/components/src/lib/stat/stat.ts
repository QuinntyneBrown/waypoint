import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * A single headline figure with its label. Applied to a `div`; nest it in a
 * `ph-card` rather than putting both on one element. Compose it from
 * `ph-stat-label`, `ph-stat-value` and an optional `ph-stat-delta`.
 */
@Component({
  selector: 'div[ph-stat]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-statcard' },
  templateUrl: './stat.html',
  styleUrl: './stat.css',
})
export class Stat {}
