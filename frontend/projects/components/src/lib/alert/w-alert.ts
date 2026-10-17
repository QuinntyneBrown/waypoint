import { ChangeDetectionStrategy, Component, ViewEncapsulation, input } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { WIcon } from '../icon/w-icon';

export type WAlertVariant = 'info' | 'success' | 'warning' | 'error' | 'danger' | 'lime';

/**
 * Tells people something that matters now: a result, a problem or a heads-up.
 * Applied to a native `div`, `p`, `section` or `aside`; the tone is not carried by
 * colour alone, so give urgent problems `icon` and `role="alert"`. With `icon` the
 * text is wrapped in a body next to the icon; without it the text is the content.
 */
@Component({
  selector: 'div[w-alert], p[w-alert], section[w-alert], aside[w-alert]',
  imports: [NgTemplateOutlet, WIcon],
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
  template: `
    @if (icon(); as name) {
      <w-icon [name]="name" iconClass="w-alert__icon" />
      <div><ng-container [ngTemplateOutlet]="body" /></div>
    } @else {
      <ng-container [ngTemplateOutlet]="body" />
    }
    <ng-template #body><ng-content /></ng-template>
  `,
  styles: `
    .w-alert {
      display: flex;
      gap: 12px;
      padding: 14px 16px;
      border-radius: var(--w-r-card);
      border: var(--w-bw) solid var(--w-border);
      background: var(--w-white);
      font-size: 0.9rem;
      line-height: 1.45;
    }
    .w-alert__icon {
      flex: none;
      width: 20px;
      height: 20px;
      margin-top: 1px;
    }
    .w-alert--success {
      background: var(--w-success-bg);
      border-color: transparent;
    }
    .w-alert--success .w-alert__icon {
      color: var(--w-success);
    }
    .w-alert--warning {
      background: var(--w-warning-bg);
      border-color: transparent;
    }
    .w-alert--warning .w-alert__icon {
      color: var(--w-warning);
    }
    .w-alert--error,
    .w-alert--danger {
      background: var(--w-error-bg);
      border-color: transparent;
    }
    .w-alert--error .w-alert__icon,
    .w-alert--danger .w-alert__icon {
      color: var(--w-error);
    }
    .w-alert--info {
      background: var(--w-info-bg);
      border-color: transparent;
    }
    .w-alert--info .w-alert__icon {
      color: var(--w-info);
    }
    .w-alert--lime {
      background: var(--w-lime-soft);
      border-color: transparent;
    }
  `,
})
export class WAlert {
  readonly variant = input<WAlertVariant | ''>('');
  /** Icon name from the icon set; leave out for a text-only alert. */
  readonly icon = input('');
}

/** The bold first line of a `w-alert`. */
@Component({
  selector: 'div[w-alert-title], p[w-alert-title]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-alert__title' },
  template: '<ng-content />',
  styles: `
    .w-alert__title {
      font-weight: 600;
      margin-bottom: 2px;
    }
  `,
})
export class WAlertTitle {}
