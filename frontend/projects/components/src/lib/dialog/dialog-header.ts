import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The top of a `ph-dialog`: a title block and, optionally, a close icon button. */
@Component({
  selector: 'div[ph-dialog-header]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-modal__header' },
  templateUrl: './dialog-header.html',
  styleUrl: './dialog-header.css',
})
export class DialogHeader {}
