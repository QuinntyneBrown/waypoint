import { ChangeDetectionStrategy, Component, ViewEncapsulation, input } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { Icon } from '../icon/icon';

export type AlertVariant = 'info' | 'success' | 'warning' | 'error' | 'danger' | 'lime';

/**
 * Tells people something that matters now: a result, a problem or a heads-up.
 * Applied to a native `div`, `p`, `section` or `aside`; the tone is not carried by
 * colour alone, so give urgent problems `icon` and `role="alert"`. With `icon` the
 * text is wrapped in a body next to the icon; without it the text is the content.
 */
@Component({
  selector: 'div[w-alert], p[w-alert], section[w-alert], aside[w-alert]',
  imports: [NgTemplateOutlet, Icon],
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'w-alert',
    '[class.w-alert--info]': "variant() === 'info'",
    '[class.w-alert--success]': "variant() === 'success'",
    '[class.w-alert--warning]': "variant() === 'warning'",
    '[class.w-alert--error]': "variant() === 'error'",
    '[class.w-alert--danger]': "variant() === 'danger'",
    '[class.w-alert--lime]': "variant() === 'lime'",
  },
  templateUrl: './alert.html',
  styleUrl: './alert.css',
})
export class Alert {
  readonly variant = input<AlertVariant | ''>('');
  /** Icon name from the icon set; leave out for a text-only alert. */
  readonly icon = input('');
}
