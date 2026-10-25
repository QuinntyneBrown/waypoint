import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The heading of a `ph-dialog`. */
@Component({
  selector: 'h2[ph-dialog-title]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-modal__title' },
  templateUrl: './dialog-title.html',
  styleUrl: './dialog-title.css',
})
export class DialogTitle {}
