import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** A horizontal rule that separates groups of content within a page or a card. */
@Component({
  selector: 'div[ph-divider]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-divider' },
  templateUrl: './divider.html',
  styleUrl: './divider.css',
})
export class Divider {}
