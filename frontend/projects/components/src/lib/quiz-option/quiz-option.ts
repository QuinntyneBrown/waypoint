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
  selector: 'label[ph-quiz-option], li[ph-quiz-option]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'ph-quiz__opt',
    '[class.is-selected]': 'selected()',
    '[class.is-correct]': 'correct()',
  },
  templateUrl: './quiz-option.html',
  styleUrl: './quiz-option.css',
})
export class QuizOption {
  readonly selected = input(false, { transform: booleanAttribute });
  readonly correct = input(false, { transform: booleanAttribute });
}
