import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * Single-line text input: text, email, password, date and number. Applied to a native input; pair it with a label in a form field.
 */
@Component({
  selector: 'input[w-text-field]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-input' },
  templateUrl: './text-field.html',
  styleUrl: './text-field.css',
})
export class TextField {}
