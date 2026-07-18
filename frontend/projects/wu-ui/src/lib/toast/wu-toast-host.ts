import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { WuIcon } from '../icon/wu-icon';
import { ToastService, WU_TOAST_ICON } from './toast.service';

/**
 * Renders the active toast queue in the fixed `.wu-toasts` region. Mount once
 * near the app root. `aria-live="polite"` announces toasts to assistive tech.
 */
@Component({
  selector: 'wu-toast-host',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuIcon],
  template: `<div class="wu-toasts" role="region" aria-live="polite" aria-label="Notifications">
    @for (t of toasts.toasts(); track t.id) {
      <div class="wu-toast wu-toast--{{ t.type }}">
        <span class="wu-toast__icon"><wu-icon [name]="iconFor(t.type)" /></span>
        <div class="wu-toast__body">
          @if (t.title) {
            <div class="wu-toast__title">{{ t.title }}</div>
          }
          {{ t.message }}
        </div>
        <button
          class="wu-toast__close"
          type="button"
          aria-label="Dismiss"
          (click)="toasts.dismiss(t.id)"
        >
          <wu-icon name="x" />
        </button>
      </div>
    }
  </div>`,
})
export class WuToastHost {
  protected readonly toasts = inject(ToastService);
  protected iconFor(type: keyof typeof WU_TOAST_ICON): string {
    return WU_TOAST_ICON[type];
  }
}
