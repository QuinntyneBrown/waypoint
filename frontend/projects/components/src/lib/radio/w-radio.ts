import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * Lets people pick one option from a set. Applied to a native `label` that wraps
 * a native `input[type=radio]` followed by a `w-radio-box`, so the group, arrow-key
 * movement and the accessible name stay native.
 */
@Component({
  selector: 'label[w-radio]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-radio' },
  template: '<ng-content />',
  styles: `
    .w-radio {
      display: flex;
      align-items: center;
      gap: 10px;
      cursor: pointer;
      font-size: 0.95rem;
      margin-bottom: 10px;
      min-height: 44px;
    }
    .w-radio input {
      position: absolute;
      opacity: 0;
      width: 0;
      height: 0;
    }
    .w-radio input:checked + .w-radio__box {
      border-color: var(--w-ink);
    }
    .w-radio input:checked + .w-radio__box::after {
      content: '';
      width: 10px;
      height: 10px;
      border-radius: 50%;
      background: var(--w-ink);
    }
    .w-radio input:focus-visible + .w-radio__box {
      outline: 2px solid var(--w-ink);
      outline-offset: 2px;
    }
  `,
})
export class WRadio {}

/** The visible circle of a `w-radio`; leave it empty. */
@Component({
  selector: 'span[w-radio-box]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-radio__box' },
  template: '<ng-content />',
  styles: `
    .w-radio__box {
      flex: none;
      width: 20px;
      height: 20px;
      border: var(--w-bw) solid var(--w-border-strong);
      background: var(--w-white);
      display: grid;
      place-items: center;
      margin-top: 0;
      border-radius: 50%;
      transition:
        background 0.15s,
        border-color 0.15s;
    }
  `,
})
export class WRadioBox {}
