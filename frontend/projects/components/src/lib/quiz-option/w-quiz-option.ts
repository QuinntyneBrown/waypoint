import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  booleanAttribute,
  input,
} from '@angular/core';

/**
 * One answer choice in a quiz. Applied to a `label` that wraps a native radio or
 * checkbox (an attempt) or to a `li` (a review). `selected` shows the chosen
 * answer and `correct` marks the right one; say so in text as well, not by colour
 * alone.
 */
@Component({
  selector: 'label[w-quiz-option], li[w-quiz-option]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'w-quiz__opt',
    '[class.is-selected]': 'selected()',
    '[class.is-correct]': 'correct()',
  },
  template: '<ng-content />',
  styles: `
    .w-quiz__opt {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 15px 16px;
      border: var(--w-bw) solid var(--w-border-strong);
      border-radius: var(--w-r-card);
      cursor: pointer;
      margin-bottom: 10px;
      font-size: 1rem;
      transition:
        border-color 0.15s,
        background 0.15s;
    }
    .w-quiz__opt:hover {
      border-color: var(--w-ink);
    }
    .w-quiz__opt.is-selected {
      border-color: var(--w-ink);
      background: var(--w-lime-soft);
    }
    .w-quiz__opt.is-correct {
      border-color: var(--w-success);
      background: var(--w-success-bg);
    }
  `,
})
export class WQuizOption {
  readonly selected = input(false, { transform: booleanAttribute });
  readonly correct = input(false, { transform: booleanAttribute });
}
