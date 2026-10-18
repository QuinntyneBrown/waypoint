import {
  afterRenderEffect,
  ChangeDetectionStrategy,
  Component,
  computed,
  ElementRef,
  input,
  viewChild,
} from '@angular/core';
import { W_ICONS } from './icons';

/**
 * Renders a Waypoint line icon by name as an inline `<svg class="w-icon">`.
 *
 * The glyph markup (a build-time constant from W_ICONS, never user input) is
 * written to the svg via a browser-only `afterRenderEffect`. This keeps the
 * component SSR-safe — the server renders an empty svg (no innerHTML on the
 * server DOM), the browser fills it after hydration — and avoids any
 * sanitizer bypass for user content.
 */
@Component({
  selector: 'w-icon',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { style: 'display: contents' },
  templateUrl: './icon.html',
  styleUrl: './icon.css',
})
export class Icon {
  /** Icon name, e.g. 'home', 'chevron-right'. Unknown names render an empty glyph. */
  readonly name = input.required<string>();
  /** Extra classes applied to the inner svg (e.g. 'w-alert__icon'). */
  readonly iconClass = input('');

  private readonly svg = viewChild.required<ElementRef<SVGElement>>('svg');

  protected readonly classes = computed(() => `w-icon ${this.iconClass()}`.trim());

  constructor() {
    afterRenderEffect(() => {
      this.svg().nativeElement.innerHTML = W_ICONS[this.name()] ?? '';
    });
  }
}
