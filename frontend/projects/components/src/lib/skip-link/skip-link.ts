import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * Lets keyboard users jump past repeated navigation. Applied to a native `a` whose
 * `href` points at the main content (`#main`). It is off-screen until it is focused,
 * so make it the first focusable element on the page.
 */
@Component({
  selector: 'a[ph-skip-link]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-skip-link' },
  templateUrl: './skip-link.html',
  styleUrl: './skip-link.css',
})
export class SkipLink {}
