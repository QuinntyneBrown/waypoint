import { Injectable, signal } from '@angular/core';

export type WuToastType = 'success' | 'error' | 'warning' | 'info' | 'default';

export interface WuToast {
  readonly id: number;
  readonly message: string;
  readonly type: WuToastType;
  readonly title?: string;
  /** Auto-dismiss delay in ms; 0 = sticky (dismiss only on user action). */
  readonly duration: number;
}

export interface WuToastOptions {
  type?: WuToastType;
  title?: string;
  duration?: number;
}

const DEFAULT_DURATION = 3600;

/** Icon per toast type, mirroring the mock (`wordup.js`). */
export const WU_TOAST_ICON: Record<WuToastType, string> = {
  success: 'check-circle',
  error: 'x-circle',
  warning: 'alert-triangle',
  info: 'info',
  default: 'info',
};

/**
 * Signal-based toast queue. Zoneless-friendly: `show`/`dismiss` mutate a signal,
 * which drives the WuToastHost's rendering and change detection.
 */
@Injectable({ providedIn: 'root' })
export class ToastService {
  private seq = 0;
  private readonly timers = new Map<number, ReturnType<typeof setTimeout>>();
  private readonly _toasts = signal<readonly WuToast[]>([]);
  readonly toasts = this._toasts.asReadonly();

  show(message: string, opts: WuToastOptions = {}): number {
    const id = ++this.seq;
    const duration = opts.duration ?? DEFAULT_DURATION;
    const toast: WuToast = {
      id,
      message,
      type: opts.type ?? 'default',
      title: opts.title,
      duration,
    };
    this._toasts.update((list) => [...list, toast]);
    if (duration !== 0) {
      this.timers.set(
        id,
        setTimeout(() => this.dismiss(id), duration),
      );
    }
    return id;
  }

  success(message: string, opts: Omit<WuToastOptions, 'type'> = {}): number {
    return this.show(message, { ...opts, type: 'success' });
  }
  error(message: string, opts: Omit<WuToastOptions, 'type'> = {}): number {
    return this.show(message, { ...opts, type: 'error' });
  }
  warning(message: string, opts: Omit<WuToastOptions, 'type'> = {}): number {
    return this.show(message, { ...opts, type: 'warning' });
  }
  info(message: string, opts: Omit<WuToastOptions, 'type'> = {}): number {
    return this.show(message, { ...opts, type: 'info' });
  }

  dismiss(id: number): void {
    const timer = this.timers.get(id);
    if (timer) {
      clearTimeout(timer);
      this.timers.delete(id);
    }
    this._toasts.update((list) => list.filter((t) => t.id !== id));
  }

  clear(): void {
    for (const timer of this.timers.values()) clearTimeout(timer);
    this.timers.clear();
    this._toasts.set([]);
  }
}
