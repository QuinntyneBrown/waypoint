import { ChangeDetectionStrategy, Component, ViewEncapsulation, input } from '@angular/core';

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
  template: '<ng-content />',
  styles: `
    .w-statcard {
      padding: var(--w-s5);
    }
  `,
})
export class WStat {}

/** The caption above a stat's figure. Also works outside a `w-stat` in compact layouts. */
@Component({
  selector: 'div[w-stat-label], span[w-stat-label]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-statcard__label' },
  template: '<ng-content />',
  styles: `
    .w-statcard__label {
      font-family: var(--w-font-head);
      text-transform: uppercase;
      letter-spacing: 0.07em;
      font-size: 0.7rem;
      font-weight: 600;
      color: var(--w-muted);
    }
  `,
})
export class WStatLabel {}

/** The large figure of a stat. Also works outside a `w-stat` in compact layouts. */
@Component({
  selector: 'div[w-stat-value], span[w-stat-value], strong[w-stat-value]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-statcard__value' },
  template: '<ng-content />',
  styles: `
    .w-statcard__value {
      font-family: var(--w-font-head);
      font-weight: 500;
      font-size: 2.1rem;
      line-height: 1.1;
      margin-top: 6px;
      letter-spacing: -0.02em;
    }
  `,
})
export class WStatValue {}

/** A smaller line under the figure; `trend` colours it for a rise or a fall. */
@Component({
  selector: 'div[w-stat-delta]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'w-statcard__delta',
    '[class.is-up]': "trend() === 'up'",
    '[class.is-down]': "trend() === 'down'",
  },
  template: '<ng-content />',
  styles: `
    .w-statcard__delta {
      font-size: 0.8rem;
      font-weight: 600;
      margin-top: 4px;
      display: inline-flex;
      align-items: center;
      gap: 4px;
    }
    .w-statcard__delta.is-up {
      color: var(--w-success);
    }
    .w-statcard__delta.is-down {
      color: var(--w-error);
    }
  `,
})
export class WStatDelta {
  readonly trend = input<'up' | 'down' | ''>('');
}
