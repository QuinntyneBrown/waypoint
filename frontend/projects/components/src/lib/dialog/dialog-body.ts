import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The content area of a `ph-dialog`. */
@Component({
  selector: 'div[ph-dialog-body]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-modal__body' },
  templateUrl: './dialog-body.html',
  styleUrl: './dialog-body.css',
})
export class DialogBody {}
