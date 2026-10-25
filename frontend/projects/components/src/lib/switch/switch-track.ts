import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The visible track and thumb of a `ph-switch`; leave it empty. */
@Component({
  selector: 'span[ph-switch-track]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-switch__track' },
  templateUrl: './switch-track.html',
  styleUrl: './switch-track.css',
})
export class SwitchTrack {}
