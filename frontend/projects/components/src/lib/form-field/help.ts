import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** Muted caption under a control. Link it with `aria-describedby`. */
@Component({
  selector: 'p[ph-help], div[ph-help], span[ph-help]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-help' },
  templateUrl: './help.html',
  styleUrl: './help.css',
})
export class Help {}
