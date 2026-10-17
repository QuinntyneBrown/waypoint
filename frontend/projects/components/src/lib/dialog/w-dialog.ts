import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  inject,
  input,
  output,
} from '@angular/core';
import { WAccessibleDialog } from './w-accessible-dialog';

/**
 * Interrupts the page to ask for a decision or short input. Render it only while
 * it is open (`@if`): it draws a scrim and a panel, traps focus, returns focus to
 * the opener when it is destroyed, and emits `dismiss` when people press Escape or
 * click the scrim. Name it with `aria-labelledby` (the `w-dialog-title` id) or
 * `aria-label`. Compose the panel from `w-dialog-header`, `w-dialog-body` and
 * `w-dialog-footer`.
 */
@Component({
  selector: 'div[w-dialog]',
  hostDirectives: [WAccessibleDialog],
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'w-modal is-open',
    '[class.w-modal--lg]': "size() === 'lg'",
    '[class.w-modal--sm]': "size() === 'sm'",
  },
  template: `
    <div class="w-modal__scrim" (click)="dismiss.emit()"></div>
    <div class="w-modal__panel"><ng-content /></div>
  `,
  styles: `
    .w-modal {
      position: fixed;
      inset: 0;
      z-index: 80;
      display: none;
      align-items: center;
      justify-content: center;
      padding: var(--w-s4);
    }
    .w-modal.is-open {
      display: flex;
      animation: w-fade 0.18s ease;
    }
    .w-modal__scrim {
      position: absolute;
      inset: 0;
      background: var(--color-bg-backdrop);
    }
    .w-modal__panel {
      position: relative;
      background: var(--w-white);
      border-radius: var(--w-r-lg);
      width: 100%;
      max-width: 480px;
      max-height: 90vh;
      overflow: auto;
      box-shadow: var(--w-shadow-pop);
      animation: w-pop 0.2s ease;
    }
    .w-modal--lg .w-modal__panel {
      max-width: 720px;
    }
    .w-modal--sm .w-modal__panel {
      max-width: 400px;
    }
  `,
})
export class WDialog {
  readonly size = input<'sm' | 'lg' | ''>('');
  /** Escape was pressed or the scrim was clicked. */
  readonly dismiss = output<void>();

  constructor() {
    inject(WAccessibleDialog).escape.subscribe(() => this.dismiss.emit());
  }
}

/** The top of a `w-dialog`: a title block and, optionally, a close icon button. */
@Component({
  selector: 'div[w-dialog-header]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-modal__header' },
  template: '<ng-content />',
  styles: `
    .w-modal__header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: var(--w-s3);
      padding: var(--w-s5) var(--w-s6) var(--w-s3);
    }
    .w-modal__header > button {
      flex: none;
    }
  `,
})
export class WDialogHeader {}

/** The heading of a `w-dialog`. */
@Component({
  selector: 'h2[w-dialog-title]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-modal__title' },
  template: '<ng-content />',
  styles: `
    .w-modal__title {
      margin: 0;
      font-size: 1.25rem;
    }
  `,
})
export class WDialogTitle {}

/** One line under the title that says what the dialog is for. */
@Component({
  selector: 'p[w-dialog-subtitle]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-modal__sub' },
  template: '<ng-content />',
  styles: `
    .w-modal__sub {
      color: var(--w-muted);
      font-size: 0.9rem;
      margin: 4px 0 0;
    }
  `,
})
export class WDialogSubtitle {}

/** The content area of a `w-dialog`. */
@Component({
  selector: 'div[w-dialog-body]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-modal__body' },
  template: '<ng-content />',
  styles: `
    .w-modal__body {
      padding: 0 var(--w-s6) var(--w-s5);
    }
  `,
})
export class WDialogBody {}

/** The action row of a `w-dialog`; the confirming action goes last. */
@Component({
  selector: 'div[w-dialog-footer]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-modal__footer' },
  template: '<ng-content />',
  styles: `
    .w-modal__footer {
      padding: var(--w-s4) var(--w-s6) var(--w-s6);
      display: flex;
      gap: var(--w-s3);
      justify-content: flex-end;
      flex-wrap: wrap;
    }
  `,
})
export class WDialogFooter {}
