import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The explanation under the title of a `w-empty-state`. */
@Component({
  selector: 'p[w-empty-state-text]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-empty__text' },
  templateUrl: './empty-state-text.html',
  styleUrl: './empty-state-text.css',
})
export class EmptyStateText {}
