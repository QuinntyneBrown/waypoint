import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * Native dropdown with a drawn chevron. Applied to a native select; pair it with a label in a form field.
 */
@Component({
  selector: 'select[w-select]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-select' },
  template: '<ng-content />',
  styles: `
    .w-select {
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
      background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='%2316160c' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpolyline points='6 9 12 15 18 9'/%3E%3C/svg%3E");
      background-repeat: no-repeat;
      background-position: right 12px center;
      padding-right: 38px;
    }
    .w-select:focus {
      outline: none;
      border-color: var(--w-ink);
      box-shadow: 0 0 0 3px var(--w-lime-soft);
    }
  `,
})
export class WSelect {}
