import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * A round, borderless button holding one icon. Applied to a native button or a; the consumer supplies aria-label.
 */
@Component({
  selector: 'button[w-icon-button], a[w-icon-button]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-iconbtn' },
  templateUrl: './icon-button.html',
  styleUrl: './icon-button.css',
})
export class IconButton {}
