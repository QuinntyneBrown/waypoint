import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The heading of a `w-dialog`. */
@Component({
  selector: 'h2[w-dialog-title]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-modal__title' },
  templateUrl: './dialog-title.html',
  styleUrl: './dialog-title.css',
})
export class DialogTitle {}
