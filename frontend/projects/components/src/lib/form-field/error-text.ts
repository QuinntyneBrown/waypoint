import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** Red message under a control. Render it only while the control is invalid. */
@Component({
  selector: 'p[ph-error-text], div[ph-error-text]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-error-text' },
  templateUrl: './error-text.html',
  styleUrl: './error-text.css',
})
export class ErrorText {}
