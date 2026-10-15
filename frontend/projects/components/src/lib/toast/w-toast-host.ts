import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { WIcon } from '../icon/w-icon';
import { ToastService, W_TOAST_ICON } from './toast.service';

/**
 * Renders the active toast queue in the fixed `.w-toasts` region. Mount once
 * near the app root. `aria-live="polite"` announces toasts to assistive tech.
 */
@Component({
  selector: 'w-toast-host',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WIcon],
  template: `<div class="w-toasts" role="region" aria-live="polite" aria-label="Notifications">
    @for (t of toasts.toasts(); track t.id) {
      <div class="w-toast w-toast--{{ t.type }}">
        <span class="w-toast__icon"><w-icon [name]="iconFor(t.type)" /></span>
        <div class="w-toast__body">
          @if (t.title) {
            <div class="w-toast__title">{{ t.title }}</div>
          }
          {{ t.message }}
        </div>
        <button
          class="w-toast__close"
          type="button"
          aria-label="Dismiss"
          (click)="toasts.dismiss(t.id)"
        >
          <w-icon name="x" />
        </button>
      </div>
    }
  </div>`,
})
export class WToastHost {
  protected readonly toasts = inject(ToastService);
  protected iconFor(type: keyof typeof W_TOAST_ICON): string {
    return W_TOAST_ICON[type];
  }
}
