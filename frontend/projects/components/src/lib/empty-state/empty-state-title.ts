import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The heading of a `ph-empty-state`; use the heading level that fits the page outline. */
@Component({
  selector:
    'h1[ph-empty-state-title], h2[ph-empty-state-title], h3[ph-empty-state-title], p[ph-empty-state-title]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-empty__title' },
  templateUrl: './empty-state-title.html',
  styleUrl: './empty-state-title.css',
})
export class EmptyStateTitle {}
