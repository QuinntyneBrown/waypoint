import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The dark page footer of the public site. Applied to a native `footer`. */
@Component({
  selector: 'footer[ph-footer]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-footer' },
  templateUrl: './footer.html',
  styleUrl: './footer.css',
})
export class Footer {}
