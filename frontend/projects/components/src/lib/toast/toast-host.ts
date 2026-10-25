import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Icon } from '../icon/icon';
import { ToastService, W_TOAST_ICON } from './toast.service';

/**
 * Renders the active toast queue in the fixed `.ph-toasts` region. Mount once
 * near the app root. `aria-live="polite"` announces toasts to assistive tech.
 */
@Component({
  selector: 'ph-toast-host',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Icon],
  templateUrl: './toast-host.html',
  styleUrl: './toast-host.css',
})
export class ToastHost {
  protected readonly toasts = inject(ToastService);
  protected iconFor(type: keyof typeof W_TOAST_ICON): string {
    return W_TOAST_ICON[type];
  }
}
