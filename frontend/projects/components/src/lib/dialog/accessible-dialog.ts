import { DOCUMENT } from '@angular/common';
import { AfterViewInit, Directive, ElementRef, OnDestroy, inject, output } from '@angular/core';

const TABBABLE_SELECTOR = [
  'a[href]',
  'area[href]',
  'button:not([disabled])',
  'input:not([disabled]):not([type="hidden"])',
  'select:not([disabled])',
  'textarea:not([disabled])',
  'iframe',
  'object',
  'embed',
  '[contenteditable="true"]',
  '[tabindex]:not([tabindex="-1"])',
].join(',');

/**
 * Adds modal semantics and complete keyboard focus management to an overlay.
 *
 * Place `phDialogInitialFocus` on a descendant when the first tabbable control
 * is not the most useful initial target. Consumers close the overlay in
 * response to `phDialogEscape`; destruction restores focus to its opener.
 */
@Directive({
  selector: '[phAccessibleDialog]',
  standalone: true,
  host: {
    role: 'dialog',
    'aria-modal': 'true',
    tabindex: '-1',
    '(keydown)': 'onKeydown($event)',
  },
})
export class AccessibleDialog implements AfterViewInit, OnDestroy {
  readonly escape = output<void>({ alias: 'phDialogEscape' });

  private readonly element = inject<ElementRef<HTMLElement>>(ElementRef).nativeElement;
  private readonly document = inject(DOCUMENT);
  private readonly opener = this.activeHTMLElement();
  private destroyed = false;

  ngAfterViewInit(): void {
    queueMicrotask(() => {
      if (!this.destroyed) this.focusInitialTarget();
    });
  }

  ngOnDestroy(): void {
    this.destroyed = true;
    const opener = this.opener;
    queueMicrotask(() => {
      if (opener?.isConnected) opener.focus({ preventScroll: true });
    });
  }

  protected onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Escape') {
      event.preventDefault();
      event.stopPropagation();
      this.escape.emit();
      return;
    }

    if (event.key !== 'Tab') return;

    const tabbable = this.tabbableElements();
    if (tabbable.length === 0) {
      event.preventDefault();
      this.element.focus({ preventScroll: true });
      return;
    }

    const active = this.activeHTMLElement();
    const first = tabbable[0];
    const last = tabbable[tabbable.length - 1];
    const movingBeforeFirst =
      event.shiftKey && (active === first || !this.element.contains(active));
    const movingPastLast = !event.shiftKey && (active === last || !this.element.contains(active));

    if (movingBeforeFirst || movingPastLast) {
      event.preventDefault();
      (movingBeforeFirst ? last : first).focus({ preventScroll: true });
    }
  }

  private focusInitialTarget(): void {
    const requested = this.element.querySelector<HTMLElement>(
      '[phDialogInitialFocus], [autofocus]',
    );
    const target = requested && this.isTabbable(requested) ? requested : this.tabbableElements()[0];
    (target ?? this.element).focus({ preventScroll: true });
  }

  private tabbableElements(): HTMLElement[] {
    return Array.from(this.element.querySelectorAll<HTMLElement>(TABBABLE_SELECTOR)).filter(
      (candidate) => this.isTabbable(candidate),
    );
  }

  private isTabbable(candidate: HTMLElement): boolean {
    if (candidate.tabIndex < 0) return false;
    if (candidate.matches(':disabled')) return false;
    if (candidate.closest('[hidden], [inert], [aria-hidden="true"]')) return false;
    const styles = this.document.defaultView?.getComputedStyle(candidate);
    return styles?.display !== 'none' && styles?.visibility !== 'hidden';
  }

  private activeHTMLElement(): HTMLElement | null {
    const active = this.document.activeElement;
    return active && typeof (active as HTMLElement).focus === 'function'
      ? (active as HTMLElement)
      : null;
  }
}
