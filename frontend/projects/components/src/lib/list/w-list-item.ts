import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** One row of a list: a title with supporting detail, separated from its neighbours by a rule. */
@Component({
  selector: 'div[w-list-item], li[w-list-item]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-listitem' },
  template: '<ng-content />',
  styles: `
    .w-listitem {
      display: flex;
      align-items: center;
      gap: 14px;
      padding: 14px 0;
      border-bottom: var(--w-bw) solid var(--w-border);
    }
    .w-listitem:last-child {
      border-bottom: 0;
    }
  `,
})
export class WListItem {}

/** The text column of a `w-list-item`; it takes the remaining width. */
@Component({
  selector: 'div[w-list-item-body]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-listitem__body' },
  template: '<ng-content />',
  styles: `
    .w-listitem__body {
      min-width: 0;
      flex: 1;
    }
  `,
})
export class WListItemBody {}

/** The first line of a `w-list-item`. */
@Component({
  selector: 'div[w-list-item-title]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-listitem__title' },
  template: '<ng-content />',
  styles: `
    .w-listitem__title {
      font-weight: 600;
      line-height: 1.25;
    }
  `,
})
export class WListItemTitle {}

/** The quieter second line of a `w-list-item`. */
@Component({
  selector: 'div[w-list-item-meta]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-listitem__meta' },
  template: '<ng-content />',
  styles: `
    .w-listitem__meta {
      font-size: 0.82rem;
      color: var(--w-muted);
    }
  `,
})
export class WListItemMeta {}
