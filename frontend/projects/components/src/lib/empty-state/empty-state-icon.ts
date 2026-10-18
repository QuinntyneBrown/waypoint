import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The round icon badge at the top of a `w-empty-state`; put a `w-icon` inside. */
@Component({
  selector: 'div[w-empty-state-icon]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-empty__icon' },
  templateUrl: './empty-state-icon.html',
  styleUrl: './empty-state-icon.css',
})
export class EmptyStateIcon {}
