import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * Turns a setting on or off straight away. Applied to a native `label` that wraps
 * a native `input[type=checkbox]` followed by a `w-switch-track`, so forms, focus
 * and the accessible name stay native.
 */
@Component({
  selector: 'label[w-switch]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-switch' },
  template: '<ng-content />',
  styles: `
    .w-switch {
      display: inline-flex;
      align-items: center;
      gap: 10px;
      cursor: pointer;
    }
    .w-switch input {
      position: absolute;
      opacity: 0;
    }
    .w-switch input:checked + .w-switch__track {
      background: var(--w-ink);
    }
    .w-switch input:checked + .w-switch__track::after {
      transform: translateX(18px);
    }
    .w-switch input:focus-visible + .w-switch__track {
      outline: 2px solid var(--w-ink);
      outline-offset: 2px;
    }
  `,
})
export class WSwitch {}

/** The visible track and thumb of a `w-switch`; leave it empty. */
@Component({
  selector: 'span[w-switch-track]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-switch__track' },
  template: '<ng-content />',
  styles: `
    .w-switch__track {
      width: 42px;
      height: 24px;
      border-radius: var(--w-r-pill);
      background: var(--w-g400);
      position: relative;
      transition: background 0.18s;
    }
    .w-switch__track::after {
      content: '';
      position: absolute;
      top: 2px;
      left: 2px;
      width: 20px;
      height: 20px;
      border-radius: 50%;
      background: var(--w-white);
      transition: transform 0.18s;
    }
  `,
})
export class WSwitchTrack {}
