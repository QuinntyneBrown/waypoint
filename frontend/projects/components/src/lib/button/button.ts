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
 * and accessibility stay native. The `ph-btn*` classes are set on the host as an
 * implementation detail; apps use the inputs instead of writing the classes.
 *
 * `busy` shows a spinner, keeps the width and focus, and ignores activation
 * without using native `disabled`.
 */
@Component({
  selector: 'button[ph-button], a[ph-button]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'ph-btn',
    '[class.ph-btn--primary]': "variant() === 'primary'",
    '[class.ph-btn--secondary]': "variant() === 'secondary'",
    '[class.ph-btn--ghost]': "variant() === 'ghost'",
    '[class.ph-btn--subtle]': "variant() === 'subtle'",
    '[class.ph-btn--danger]': "variant() === 'danger'",
    '[class.ph-btn--link]': "variant() === 'link'",
    '[class.ph-btn--sm]': "size() === 'sm'",
    '[class.ph-btn--lg]': "size() === 'lg'",
    '[class.ph-btn--block]': 'block()',
    '[class.ph-btn--icon]': 'iconOnly()',
    '[class.is-loading]': 'busy()',
    '[attr.aria-busy]': 'busy() ? "true" : null',
  },
  templateUrl: './button.html',
  styleUrl: './button.css',
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
