import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * Lets people turn one option on or off. Applied to a native `label` that wraps
 * a native `input[type=checkbox]` followed by a `w-checkbox-box`, so forms, focus
 * and the accessible name stay native.
 */
@Component({
  selector: 'label[w-checkbox]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-check' },
  template: '<ng-content />',
  styles: `
    .w-check {
      display: flex;
      align-items: center;
      gap: 10px;
      cursor: pointer;
      font-size: 0.95rem;
      margin-bottom: 10px;
      min-height: 44px;
    }
    .w-check input {
      position: absolute;
      opacity: 0;
      width: 0;
      height: 0;
    }
    .w-check input:checked + .w-check__box {
      background: var(--w-lime);
      border-color: var(--w-lime);
    }
    .w-check input:checked + .w-check__box .w-icon {
      opacity: 1;
    }
    .w-check input:checked + .w-check__box:empty::after {
      opacity: 1;
    }
    .w-check input:focus-visible + .w-check__box {
      outline: 2px solid var(--w-ink);
      outline-offset: 2px;
    }
  `,
})
export class WCheckbox {}

/** The visible square of a `w-checkbox`; put a check icon inside or leave it empty. */
@Component({
  selector: 'span[w-checkbox-box]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-check__box' },
  template: '<ng-content />',
  styles: `
    .w-check__box {
      flex: none;
      width: 20px;
      height: 20px;
      border: var(--w-bw) solid var(--w-border-strong);
      background: var(--w-white);
      display: grid;
      place-items: center;
      margin-top: 0;
      border-radius: 5px;
      transition:
        background 0.15s,
        border-color 0.15s;
    }
    .w-check__box .w-icon {
      width: 14px;
      height: 14px;
      color: var(--w-on-lime);
      opacity: 0;
    }
    .w-check__box:empty::after {
      content: '';
      width: 5px;
      height: 10px;
      border: solid var(--w-on-lime);
      border-width: 0 2px 2px 0;
      transform: rotate(45deg) translate(-1px, -1px);
      opacity: 0;
    }
  `,
})
export class WCheckboxBox {}
