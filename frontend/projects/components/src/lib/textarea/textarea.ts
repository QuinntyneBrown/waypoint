import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * Multi-line text input. Applied to a native textarea; pair it with a label in a form field.
 */
@Component({
  selector: 'textarea[w-textarea]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-textarea' },
  templateUrl: './textarea.html',
  styleUrl: './textarea.css',
})
export class Textarea {}
