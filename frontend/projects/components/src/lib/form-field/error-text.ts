import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** Red message under a control. Render it only while the control is invalid. */
@Component({
  selector: 'p[w-error-text], div[w-error-text]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-error-text' },
  templateUrl: './error-text.html',
  styleUrl: './error-text.css',
})
export class ErrorText {}
