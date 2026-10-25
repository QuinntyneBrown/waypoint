import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  booleanAttribute,
  input,
} from '@angular/core';

/**
 * Takes people somewhere: another page, a section, a file or an external site.
 * Applied to a native `a`, so `href` and `routerLink` stay native. Inline links
 * are underlined; `standalone` links are underlined only on hover.
 */
@Component({
  selector: 'a[ph-link]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-link', '[class.ph-link--standalone]': 'standalone()' },
  templateUrl: './link.html',
  styleUrl: './link.css',
})
export class Link {
  readonly standalone = input(false, { transform: booleanAttribute });
}
