import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';

/**
 * A round, borderless button holding one icon. Applied to a native button or a; the consumer supplies aria-label.
 */
@Component({
  selector: 'button[w-icon-button], a[w-icon-button]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-iconbtn' },
  template: '<ng-content />',
  styles: `
    .w-iconbtn {
      width: 44px;
      height: 44px;
      border-radius: var(--w-r-pill);
      border: 0;
      background: transparent;
      color: var(--w-ink);
      display: inline-grid;
      place-items: center;
      cursor: pointer;
      position: relative;
      flex: none;
    }
    .w-iconbtn:hover {
      background: var(--w-g200);
    }
    .w-iconbtn .w-icon {
      width: 20px;
      height: 20px;
    }
    .w-iconbtn:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
    .w-iconbtn:disabled:hover {
      background: transparent;
    }
  `,
})
export class IconButton {}
