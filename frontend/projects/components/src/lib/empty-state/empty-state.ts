import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * Explains why a region has no content and what to do next. Applied to a native
 * `div`, `section` or `li`; compose it from `ph-empty-state-icon`,
 * `ph-empty-state-title` and `ph-empty-state-text`, then add any action buttons.
 * Inside a card, nest it in the card rather than putting both on one element.
 */
@Component({
  selector: 'div[ph-empty-state], section[ph-empty-state], li[ph-empty-state]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-empty' },
  templateUrl: './empty-state.html',
  styleUrl: './empty-state.css',
})
export class EmptyState {}
