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
  template: '<ng-content />',
  styles: `
    .w-empty {
      text-align: center;
      padding: var(--w-s9) var(--w-s5);
      max-width: 420px;
      margin-inline: auto;
    }
  `,
})
export class WEmptyState {}

/** The round icon badge at the top of a `w-empty-state`; put a `w-icon` inside. */
@Component({
  selector: 'div[w-empty-state-icon]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-empty__icon' },
  template: '<ng-content />',
  styles: `
    .w-empty__icon {
      width: 64px;
      height: 64px;
      border-radius: 50%;
      background: var(--w-g100);
      display: grid;
      place-items: center;
      margin: 0 auto var(--w-s4);
    }
    .w-empty__icon .w-icon {
      width: 30px;
      height: 30px;
      color: var(--w-ink-45);
    }
  `,
})
export class WEmptyStateIcon {}

/** The heading of a `w-empty-state`; use the heading level that fits the page outline. */
@Component({
  selector:
    'h1[w-empty-state-title], h2[w-empty-state-title], h3[w-empty-state-title], p[w-empty-state-title]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-empty__title' },
  template: '<ng-content />',
  styles: `
    .w-empty__title {
      font-size: 1.2rem;
      margin-bottom: 0.35em;
    }
  `,
})
export class WEmptyStateTitle {}

/** The explanation under the title of a `w-empty-state`. */
@Component({
  selector: 'p[w-empty-state-text]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-empty__text' },
  template: '<ng-content />',
  styles: `
    .w-empty__text {
      color: var(--w-muted);
      margin-bottom: var(--w-s5);
    }
  `,
})
export class WEmptyStateText {}
