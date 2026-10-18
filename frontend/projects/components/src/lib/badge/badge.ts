import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  booleanAttribute,
  input,
} from '@angular/core';

export type BadgeVariant =
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
  templateUrl: './badge.html',
  styleUrl: './badge.css',
})
export class Badge {
  readonly variant = input<BadgeVariant | ''>('');
  readonly dot = input(false, { transform: booleanAttribute });
}
