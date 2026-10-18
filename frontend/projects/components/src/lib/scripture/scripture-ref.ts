import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** The book, chapter and verse under a `w-scripture` quote. */
@Component({
  selector: 'span[w-scripture-ref]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-scripture__ref' },
  templateUrl: './scripture-ref.html',
  styleUrl: './scripture-ref.css',
})
export class ScriptureRef {}
