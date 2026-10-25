import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  booleanAttribute,
  input,
} from '@angular/core';

/**
 * Shows labelled facts about one record. Applied to a native `dl` holding `dt`
 * terms and `dd` details. Terms sit beside details on wide screens; `vertical`
 * (or a narrow screen) stacks them.
 */
@Component({
  selector: 'dl[ph-description-list]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'ph-dl', '[class.ph-dl--vertical]': 'vertical()' },
  templateUrl: './description-list.html',
  styleUrl: './description-list.css',
})
export class DescriptionList {
  readonly vertical = input(false, { transform: booleanAttribute });
}
