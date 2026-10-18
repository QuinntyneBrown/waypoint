import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** Sets a passage or a reflection prompt apart in a serif quote block. */
@Component({
  selector: 'div[w-scripture], p[w-scripture]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-scripture' },
  template: '<ng-content />',
  styles: `
    .w-scripture {
      border-left: 3px solid var(--w-lime);
      background: var(--w-g100);
      padding: var(--w-s5) var(--w-s6);
      border-radius: 0 var(--w-r-card) var(--w-r-card) 0;
      font-family: var(--w-font-serif);
      font-size: 1.15rem;
      line-height: 1.6;
    }
  `,
})
export class Scripture {}

/** The book, chapter and verse under a `w-scripture` quote. */
@Component({
  selector: 'span[w-scripture-ref]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-scripture__ref' },
  template: '<ng-content />',
  styles: `
    .w-scripture__ref {
      display: block;
      font-family: var(--w-font-head);
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: var(--w-muted);
      margin-top: 12px;
      font-weight: 600;
    }
  `,
})
export class ScriptureRef {}
