import {
  afterRenderEffect,
  ChangeDetectionStrategy,
  Component,
  computed,
  ElementRef,
  input,
  viewChild,
} from '@angular/core';
import { WU_ICONS } from './wu-icons';

/**
 * Renders a Word Up line icon by name as an inline `<svg class="wu-icon">`.
 *
 * The glyph markup (a build-time constant from WU_ICONS, never user input) is
 * written to the svg via a browser-only `afterRenderEffect`. This keeps the
 * component SSR-safe — the server renders an empty svg (no innerHTML on the
 * server DOM), the browser fills it after hydration — and avoids any
 * sanitizer bypass (Risks #11).
 */
@Component({
  selector: 'wu-icon',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { style: 'display: contents' },
  template: `<svg
    #svg
    [class]="classes()"
    viewBox="0 0 24 24"
    aria-hidden="true"
    focusable="false"
  ></svg>`,
})
export class WuIcon {
  /** Icon name, e.g. 'home', 'chevron-right'. Unknown names render an empty glyph. */
  readonly name = input.required<string>();
  /** Extra classes applied to the inner svg (e.g. 'wu-alert__icon'). */
  readonly iconClass = input('');

  private readonly svg = viewChild.required<ElementRef<SVGElement>>('svg');

  protected readonly classes = computed(() => `wu-icon ${this.iconClass()}`.trim());

  constructor() {
    afterRenderEffect(() => {
      this.svg().nativeElement.innerHTML = WU_ICONS[this.name()] ?? '';
    });
  }
}
