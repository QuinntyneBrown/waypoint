import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The bold first line of a `w-alert`. */
@Component({
  selector: 'div[w-alert-title], p[w-alert-title]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-alert__title' },
  templateUrl: './alert-title.html',
  styleUrl: './alert-title.css',
})
export class AlertTitle {}
