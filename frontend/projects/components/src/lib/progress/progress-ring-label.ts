import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The figure in the middle of a `w-progress-ring`. */
@Component({
  selector: 'span[w-progress-ring-label]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-ring__label' },
  templateUrl: './progress-ring-label.html',
  styleUrl: './progress-ring-label.css',
})
export class ProgressRingLabel {}
