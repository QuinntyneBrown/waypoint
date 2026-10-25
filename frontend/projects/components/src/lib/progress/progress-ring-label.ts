import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The figure in the middle of a `ph-progress-ring`. */
@Component({
  selector: 'span[ph-progress-ring-label]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-ring__label' },
  templateUrl: './progress-ring-label.html',
  styleUrl: './progress-ring-label.css',
})
export class ProgressRingLabel {}
