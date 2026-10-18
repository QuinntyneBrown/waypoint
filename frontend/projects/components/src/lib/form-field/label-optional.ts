import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/** Muted "(optional)" marker inside a label. */
@Component({
  selector: 'span[w-label-optional]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-label__opt' },
  templateUrl: './label-optional.html',
  styleUrl: './label-optional.css',
})
export class LabelOptional {}
