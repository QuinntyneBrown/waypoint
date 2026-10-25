import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The action row of a `ph-dialog`; the confirming action goes last. */
@Component({
  selector: 'div[ph-dialog-footer]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-modal__footer' },
  templateUrl: './dialog-footer.html',
  styleUrl: './dialog-footer.css',
})
export class DialogFooter {}
