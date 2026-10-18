import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The content area of a `w-dialog`. */
@Component({
  selector: 'div[w-dialog-body]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-modal__body' },
  templateUrl: './dialog-body.html',
  styleUrl: './dialog-body.css',
})
export class DialogBody {}
