import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The explanation under the title of a `ph-empty-state`. */
@Component({
  selector: 'p[ph-empty-state-text]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-empty__text' },
  templateUrl: './empty-state-text.html',
  styleUrl: './empty-state-text.css',
})
export class EmptyStateText {}
