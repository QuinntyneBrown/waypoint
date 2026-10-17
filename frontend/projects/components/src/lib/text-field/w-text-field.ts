import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * Single-line text input: text, email, password, date and number. Applied to a native input; pair it with a label in a form field.
 */
@Component({
  selector: 'input[w-text-field]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-input' },
  template: '<ng-content />',
  styles: `
    .w-input {
      width: 100%;
      font-family: var(--w-font-body);
      font-size: 1rem;
      color: var(--w-ink);
      background: var(--w-white);
      border: var(--w-bw) solid var(--w-border-strong);
      border-radius: var(--w-r-input);
      padding: 11px 13px;
      line-height: 1.4;
      transition:
        border-color 0.15s,
        box-shadow 0.15s;
      -webkit-appearance: none;
      appearance: none;
    }
    .w-input::placeholder {
      color: var(--w-ink-45);
    }
    .w-input:focus {
      outline: none;
      border-color: var(--w-ink);
      box-shadow: 0 0 0 3px var(--w-lime-soft);
    }
  `,
})
export class WTextField {}
