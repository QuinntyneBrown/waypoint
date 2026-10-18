import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  booleanAttribute,
  input,
} from '@angular/core';

/** Groups a control with its label, help text and error message. */
@Component({
  selector: 'div[w-form-field], fieldset[w-form-field]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-field', '[class.is-error]': 'invalid()' },
  templateUrl: './form-field.html',
  styleUrl: './form-field.css',
})
export class FormField {
  readonly invalid = input(false, { transform: booleanAttribute });
}
