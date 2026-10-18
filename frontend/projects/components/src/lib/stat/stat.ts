import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * A single headline figure with its label. Applied to a `div`; nest it in a
 * `w-card` rather than putting both on one element. Compose it from
 * `w-stat-label`, `w-stat-value` and an optional `w-stat-delta`.
 */
@Component({
  selector: 'div[w-stat]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-statcard' },
  templateUrl: './stat.html',
  styleUrl: './stat.css',
})
export class Stat {}
