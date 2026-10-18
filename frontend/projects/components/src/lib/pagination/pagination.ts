import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * Moves between pages of a long list. Applied to a native `nav`, so it is a
 * landmark: give it an `aria-label` that says which list it pages. Put a summary
 * line and previous and next buttons inside.
 */
@Component({
  selector: 'nav[w-pagination]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-pagination' },
  template: '<ng-content />',
  styles: `
    .w-pagination {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--w-s3);
      flex-wrap: wrap;
      padding-top: var(--w-s4);
      font-size: 0.875rem;
    }
  `,
})
export class Pagination {}
