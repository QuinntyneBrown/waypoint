import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * Lets keyboard users jump past repeated navigation. Applied to a native `a` whose
 * `href` points at the main content (`#main`). It is off-screen until it is focused,
 * so make it the first focusable element on the page.
 */
@Component({
  selector: 'a[w-skip-link]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-skip-link' },
  templateUrl: './skip-link.html',
  styleUrl: './skip-link.css',
})
export class SkipLink {}
