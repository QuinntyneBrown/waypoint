import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The action row of a `w-dialog`; the confirming action goes last. */
@Component({
  selector: 'div[w-dialog-footer]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-modal__footer' },
  templateUrl: './dialog-footer.html',
  styleUrl: './dialog-footer.css',
})
export class DialogFooter {}
