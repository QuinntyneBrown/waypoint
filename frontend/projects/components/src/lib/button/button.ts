import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  ViewEncapsulation,
  booleanAttribute,
  inject,
  input,
} from '@angular/core';

export type ButtonVariant =
  'primary' | 'secondary' | 'ghost' | 'subtle' | 'neutral' | 'danger' | 'link';
export type ButtonSize = 'sm' | 'md' | 'lg';

/**
 * Starts an action. Applied to a native `button` or `a`, so forms, `routerLink`
 * and accessibility stay native. The `w-btn*` classes are set on the host as an
 * implementation detail; apps use the inputs instead of writing the classes.
 *
 * `busy` shows a spinner, keeps the width and focus, and ignores activation
 * without using native `disabled`.
 */
@Component({
  selector: 'button[w-button], a[w-button]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'w-btn',
    '[class.w-btn--primary]': "variant() === 'primary'",
    '[class.w-btn--secondary]': "variant() === 'secondary'",
    '[class.w-btn--ghost]': "variant() === 'ghost'",
    '[class.w-btn--subtle]': "variant() === 'subtle'",
    '[class.w-btn--danger]': "variant() === 'danger'",
    '[class.w-btn--link]': "variant() === 'link'",
    '[class.w-btn--sm]': "size() === 'sm'",
    '[class.w-btn--lg]': "size() === 'lg'",
    '[class.w-btn--block]': 'block()',
    '[class.w-btn--icon]': 'iconOnly()',
    '[class.is-loading]': 'busy()',
    '[attr.aria-busy]': 'busy() ? "true" : null',
  },
  template: '<ng-content />',
  styles: `
    .w-btn {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      cursor: pointer;
      font-family: var(--w-font-head);
      font-weight: 500;
      font-size: 0.95rem;
      line-height: 1;
      text-align: center;
      padding: 13px 22px;
      border-radius: var(--w-r-pill);
      border: var(--w-bw) solid transparent;
      background: var(--w-g200);
      color: var(--w-ink);
      transition:
        background 0.18s,
        color 0.18s,
        border-color 0.18s;
      white-space: nowrap;
      text-decoration: none;
      -webkit-appearance: none;
      appearance: none;
      min-height: 44px;
    }
    .w-btn:hover {
      text-decoration: none;
    }
    .w-btn .w-icon {
      width: 18px;
      height: 18px;
    }
    .w-btn--primary {
      background: var(--w-lime);
      color: var(--w-on-lime);
    }
    .w-btn--primary:hover {
      background: var(--w-lime-600);
    }
    .w-btn--secondary {
      background: var(--w-ink);
      color: var(--w-white);
    }
    .w-btn--secondary:hover {
      background: var(--color-accent-active);
    }
    .w-btn--ghost {
      background: transparent;
      border-color: var(--w-border-strong);
      color: var(--w-ink);
    }
    .w-btn--ghost:hover {
      background: var(--w-g100);
    }
    .w-btn--subtle {
      background: var(--w-g100);
      color: var(--w-ink);
    }
    .w-btn--subtle:hover {
      background: var(--w-g200);
    }
    .w-btn--danger {
      background: var(--color-danger-solid);
      color: var(--color-fg-on-danger);
    }
    .w-btn--danger:hover {
      background: var(--color-danger-solid-hover);
    }
    .w-btn--sm {
      padding: 8px 14px;
      font-size: 0.85rem;
    }
    .w-btn--lg {
      padding: 16px 28px;
      font-size: 1.05rem;
    }
    .w-btn--block {
      display: flex;
      width: 100%;
    }
    .w-btn--icon {
      padding: 11px;
      width: auto;
    }
    .w-btn:disabled,
    .w-btn.is-disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
    .w-btn.is-loading {
      color: transparent;
      position: relative;
      pointer-events: none;
    }
    .w-btn.is-loading::after {
      content: '';
      position: absolute;
      width: 16px;
      height: 16px;
      border-radius: 50%;
      border: 2px solid currentColor;
      border-top-color: transparent;
      animation: w-spin 0.7s linear infinite;
      color: var(--w-ink);
    }
    .w-btn--link {
      background: transparent;
      padding: 0;
      text-decoration: underline;
      text-underline-offset: 3px;
    }
    .w-btn--link:hover {
      background: transparent;
      text-decoration: underline;
    }
  `,
})
export class Button {
  readonly variant = input<ButtonVariant>('neutral');
  readonly size = input<ButtonSize>('md');
  readonly block = input(false, { transform: booleanAttribute });
  readonly iconOnly = input(false, { transform: booleanAttribute });
  readonly busy = input(false, { transform: booleanAttribute });

  constructor() {
    // Capture phase on the host runs before the consumer's (click) handlers.
    inject<ElementRef<HTMLElement>>(ElementRef).nativeElement.addEventListener(
      'click',
      (event) => {
        if (!this.busy()) return;
        event.preventDefault();
        event.stopImmediatePropagation();
      },
      true,
    );
  }
}
