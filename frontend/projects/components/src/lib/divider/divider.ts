import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** A horizontal rule that separates groups of content within a page or a card. */
@Component({
  selector: 'div[w-divider]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-divider' },
  templateUrl: './divider.html',
  styleUrl: './divider.css',
})
export class Divider {}
