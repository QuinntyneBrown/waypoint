import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  booleanAttribute,
  input,
} from '@angular/core';

/**
 * Shows labelled facts about one record. Applied to a native `dl` holding `dt`
 * terms and `dd` details. Terms sit beside details on wide screens; `vertical`
 * (or a narrow screen) stacks them.
 */
@Component({
  selector: 'dl[w-description-list]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-dl', '[class.w-dl--vertical]': 'vertical()' },
  template: '<ng-content />',
  styles: `
    .w-dl {
      display: grid;
      grid-template-columns: minmax(8rem, max-content) 1fr;
      gap: var(--w-s3) var(--w-s6);
      margin: 0;
    }
    .w-dl dt {
      color: var(--w-muted);
      font-size: 0.875rem;
    }
    .w-dl dd {
      margin: 0;
      font-weight: 500;
      overflow-wrap: anywhere;
    }
    .w-dl--vertical {
      grid-template-columns: 1fr;
      gap: var(--w-s1);
    }
    .w-dl--vertical dd {
      margin-bottom: var(--w-s3);
    }
    @media (max-width: 39.99rem) {
      .w-dl {
        grid-template-columns: 1fr;
        gap: var(--w-s1);
      }
      .w-dl dd {
        margin-bottom: var(--w-s3);
      }
    }
  `,
})
export class DescriptionList {
  readonly vertical = input(false, { transform: booleanAttribute });
}
