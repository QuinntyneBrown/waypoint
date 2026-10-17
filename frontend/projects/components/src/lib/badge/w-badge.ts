import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  booleanAttribute,
  input,
} from '@angular/core';

export type WBadgeVariant =
  'success' | 'warning' | 'error' | 'danger' | 'info' | 'lime' | 'ink' | 'outline';

/**
 * A short label that tells people a status or a count. Applied to a native
 * `span`, `p` or `div`. The tone is never the only signal: the text says the
 * status, and `dot` adds a marker for statuses that need one.
 */
@Component({
  selector: 'span[w-badge], p[w-badge], div[w-badge]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'w-badge',
    '[class.w-badge--success]': "variant() === 'success'",
    '[class.w-badge--warning]': "variant() === 'warning'",
    '[class.w-badge--error]': "variant() === 'error'",
    '[class.w-badge--danger]': "variant() === 'danger'",
    '[class.w-badge--info]': "variant() === 'info'",
    '[class.w-badge--lime]': "variant() === 'lime'",
    '[class.w-badge--ink]': "variant() === 'ink'",
    '[class.w-badge--outline]': "variant() === 'outline'",
    '[class.w-badge--dot]': 'dot()',
  },
  template: '<ng-content />',
  styles: `
    .w-badge {
      display: inline-flex;
      align-items: center;
      gap: 5px;
      font-family: var(--w-font-head);
      font-weight: 600;
      font-size: 0.72rem;
      letter-spacing: 0.02em;
      padding: 3px 10px;
      border-radius: var(--w-r-pill);
      background: var(--w-g200);
      color: var(--w-ink);
      white-space: nowrap;
      line-height: 1.4;
    }
    .w-badge .w-icon {
      width: 13px;
      height: 13px;
    }
    .w-badge--success {
      background: var(--w-success-bg);
      color: var(--color-success-fg);
    }
    .w-badge--warning {
      background: var(--w-warning-bg);
      color: var(--color-warning-fg);
    }
    .w-badge--error,
    .w-badge--danger {
      background: var(--w-error-bg);
      color: var(--color-danger-fg);
    }
    .w-badge--info {
      background: var(--w-info-bg);
      color: var(--color-info-fg);
    }
    .w-badge--lime {
      background: var(--w-lime);
      color: var(--w-on-lime);
    }
    .w-badge--ink {
      background: var(--w-ink);
      color: var(--w-white);
    }
    .w-badge--outline {
      background: transparent;
      border: var(--w-bw) solid var(--w-border-strong);
    }
    .w-badge--dot::before {
      content: '';
      width: 6px;
      height: 6px;
      border-radius: 50%;
      background: currentColor;
    }
  `,
})
export class WBadge {
  readonly variant = input<WBadgeVariant | ''>('');
  readonly dot = input(false, { transform: booleanAttribute });
}
