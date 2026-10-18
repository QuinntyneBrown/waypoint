import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The visible track and thumb of a `w-switch`; leave it empty. */
@Component({
  selector: 'span[w-switch-track]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-switch__track' },
  templateUrl: './switch-track.html',
  styleUrl: './switch-track.css',
})
export class SwitchTrack {}
