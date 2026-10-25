import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The bold first line of a `ph-alert`. */
@Component({
  selector: 'div[ph-alert-title], p[ph-alert-title]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-alert__title' },
  templateUrl: './alert-title.html',
  styleUrl: './alert-title.css',
})
export class AlertTitle {}
