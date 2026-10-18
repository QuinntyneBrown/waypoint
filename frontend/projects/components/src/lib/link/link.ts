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
  selector: 'a[w-link]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-link', '[class.w-link--standalone]': 'standalone()' },
  template: '<ng-content />',
  styles: `
    .w-link {
      color: var(--color-fg-link);
      text-decoration: underline;
      text-underline-offset: 3px;
      text-decoration-thickness: 1px;
      font-weight: 500;
      overflow-wrap: anywhere;
    }
    .w-link:hover {
      color: var(--color-fg-link-hover);
      text-decoration: underline;
      text-decoration-thickness: 2px;
    }
    .w-link:visited {
      color: var(--color-fg-link);
    }
    .w-link--standalone {
      display: inline-flex;
      align-items: center;
      gap: var(--w-s1);
      text-decoration: none;
    }
    .w-link--standalone:hover {
      text-decoration: underline;
    }
    .w-link .w-icon {
      width: 1em;
      height: 1em;
    }
  `,
})
export class Link {
  readonly standalone = input(false, { transform: booleanAttribute });
}
