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
  selector: 'span[ph-badge], p[ph-badge], div[ph-badge]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'ph-badge',
    '[class.ph-badge--success]': "variant() === 'success'",
    '[class.ph-badge--warning]': "variant() === 'warning'",
    '[class.ph-badge--error]': "variant() === 'error'",
    '[class.ph-badge--danger]': "variant() === 'danger'",
    '[class.ph-badge--info]': "variant() === 'info'",
    '[class.ph-badge--lime]': "variant() === 'lime'",
    '[class.ph-badge--ink]': "variant() === 'ink'",
    '[class.ph-badge--outline]': "variant() === 'outline'",
    '[class.ph-badge--dot]': 'dot()',
  },
  templateUrl: './badge.html',
  styleUrl: './badge.css',
})
export class Badge {
  readonly variant = input<BadgeVariant | ''>('');
  readonly dot = input(false, { transform: booleanAttribute });
}
