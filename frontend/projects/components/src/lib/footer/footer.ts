import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The dark page footer of the public site. Applied to a native `footer`. */
@Component({
  selector: 'footer[w-footer]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-footer' },
  template: '<ng-content />',
  styles: `
    .w-footer {
      background: var(--w-ink);
      color: var(--color-fg-inverse-muted);
      padding: var(--w-s9) 0 var(--w-s6);
    }
    .w-footer a {
      color: var(--color-fg-inverse-muted);
    }
    .w-footer a:hover {
      color: var(--w-white);
    }
  `,
})
export class Footer {}
