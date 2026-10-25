import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The round icon badge at the top of a `ph-empty-state`; put a `ph-icon` inside. */
@Component({
  selector: 'div[ph-empty-state-icon]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-empty__icon' },
  templateUrl: './empty-state-icon.html',
  styleUrl: './empty-state-icon.css',
})
export class EmptyStateIcon {}
