import { ChangeDetectionStrategy, Component, ViewEncapsulation, input } from '@angular/core';

/**
 * Shows how far along something is, as a bar. Applied to a `div`. Give it a
 * `label` and it is a `progressbar` with its value announced; leave `label` out
 * when the number is already printed next to it and add `aria-hidden="true"`.
 */
@Component({
  selector: 'div[w-progress]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'w-progress',
    '[attr.role]': "label() ? 'progressbar' : null",
    '[attr.aria-label]': 'label() || null',
    '[attr.aria-valuemin]': 'label() ? 0 : null',
    '[attr.aria-valuemax]': 'label() ? 100 : null',
    '[attr.aria-valuenow]': 'label() ? value() : null',
    '[style.--val]': "value() + '%'",
  },
  template: '<div class="w-progress__fill"></div>',
  styles: `
    .w-progress {
      width: 100%;
      height: 9px;
      border-radius: var(--w-r-pill);
      background: var(--w-g300);
      overflow: hidden;
    }
    .w-progress__fill {
      height: 100%;
      width: var(--val, 0%);
      background: var(--w-lime);
      border-radius: inherit;
      transition: width 0.4s;
    }
  `,
})
export class WProgress {
  /** Percent complete, 0 to 100. */
  readonly value = input.required<number>();
  /** Accessible name; makes the bar a `progressbar`. */
  readonly label = input('');
}

/**
 * Shows a percentage as a ring with its figure in the middle. Applied to a `div`
 * that holds a `w-progress-ring-label`.
 */
@Component({
  selector: 'div[w-progress-ring]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'w-ring',
    '[class.w-ring--lg]': "size() === 'lg'",
    '[style.--val]': 'value()',
  },
  template: '<ng-content />',
  styles: `
    .w-ring {
      --val: 0;
      --size: 72px;
      --tw: 9px;
      width: var(--size);
      height: var(--size);
      border-radius: 50%;
      flex: none;
      background: conic-gradient(var(--w-lime) calc(var(--val) * 1%), var(--w-g300) 0);
      display: grid;
      place-items: center;
      position: relative;
    }
    .w-ring::before {
      content: '';
      position: absolute;
      inset: var(--tw);
      border-radius: 50%;
      background: var(--w-white);
    }
    .w-ring--lg {
      --size: 112px;
      --tw: 12px;
    }
    .w-ring--lg .w-ring__label {
      font-size: 1.4rem;
    }
  `,
})
export class WProgressRing {
  /** Percent complete, 0 to 100. */
  readonly value = input.required<number>();
  readonly size = input<'lg' | ''>('');
}

/** The figure in the middle of a `w-progress-ring`. */
@Component({
  selector: 'span[w-progress-ring-label]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-ring__label' },
  template: '<ng-content />',
  styles: `
    .w-ring__label {
      position: relative;
      font-family: var(--w-font-head);
      font-weight: 500;
      font-size: 0.8rem;
      line-height: 1;
      text-align: center;
    }
  `,
})
export class WProgressRingLabel {}
