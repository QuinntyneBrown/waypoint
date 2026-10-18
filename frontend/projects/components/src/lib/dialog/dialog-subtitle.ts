import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** One line under the title that says what the dialog is for. */
@Component({
  selector: 'p[w-dialog-subtitle]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-modal__sub' },
  templateUrl: './dialog-subtitle.html',
  styleUrl: './dialog-subtitle.css',
})
export class DialogSubtitle {}
