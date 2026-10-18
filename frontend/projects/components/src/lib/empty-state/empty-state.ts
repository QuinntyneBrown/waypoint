import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * Explains why a region has no content and what to do next. Applied to a native
 * `div`, `section` or `li`; compose it from `w-empty-state-icon`,
 * `w-empty-state-title` and `w-empty-state-text`, then add any action buttons.
 * Inside a card, nest it in the card rather than putting both on one element.
 */
@Component({
  selector: 'div[w-empty-state], section[w-empty-state], li[w-empty-state]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-empty' },
  templateUrl: './empty-state.html',
  styleUrl: './empty-state.css',
})
export class EmptyState {}
