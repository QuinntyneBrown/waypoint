import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The book, chapter and verse under a `ph-scripture` quote. */
@Component({
  selector: 'span[ph-scripture-ref]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-scripture__ref' },
  templateUrl: './scripture-ref.html',
  styleUrl: './scripture-ref.css',
})
export class ScriptureRef {}
