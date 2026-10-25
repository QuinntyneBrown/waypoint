import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * Native dropdown with a drawn chevron. Applied to a native select; pair it with a label in a form field.
 */
@Component({
  selector: 'select[ph-select]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-select' },
  templateUrl: './select.html',
  styleUrl: './select.css',
})
export class Select {}
