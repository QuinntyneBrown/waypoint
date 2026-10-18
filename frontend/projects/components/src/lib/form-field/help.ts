import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** Muted caption under a control. Link it with `aria-describedby`. */
@Component({
  selector: 'p[w-help], div[w-help], span[w-help]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-help' },
  templateUrl: './help.html',
  styleUrl: './help.css',
})
export class Help {}
