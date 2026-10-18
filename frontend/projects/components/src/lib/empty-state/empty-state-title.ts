import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The heading of a `w-empty-state`; use the heading level that fits the page outline. */
@Component({
  selector:
    'h1[w-empty-state-title], h2[w-empty-state-title], h3[w-empty-state-title], p[w-empty-state-title]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-empty__title' },
  templateUrl: './empty-state-title.html',
  styleUrl: './empty-state-title.css',
})
export class EmptyStateTitle {}
