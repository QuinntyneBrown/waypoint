import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { WU_ICONS } from './wu-icons';

/**
 * Renders a Word Up line icon by name as an inline `<svg class="wu-icon">`,
 * matching the mock contract `<svg class="wu-icon"><use .../></svg>` visually
 * while keeping the SVG inline so it is present in SSR output.
 *
 * The inner markup comes from WU_ICONS — a build-time constant generated from
 * the design system, never user input — so bypassing sanitization here is safe
 * (Risks #11 forbids bypassSecurityTrust* on USER content only).
 */
@Component({
  selector: 'wu-icon',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { style: 'display: contents' },
  template: `<svg
    [class]="classes()"
    viewBox="0 0 24 24"
    aria-hidden="true"
    focusable="false"
    [innerHTML]="markup()"
  ></svg>`,
})
export class WuIcon {
  /** Icon name, e.g. 'home', 'chevron-right'. Unknown names render an empty glyph. */
  readonly name = input.required<string>();
  /** Extra classes applied to the inner svg (e.g. 'wu-alert__icon'). */
  readonly iconClass = input('');

  private readonly sanitizer = inject(DomSanitizer);

  protected readonly classes = computed(() => `wu-icon ${this.iconClass()}`.trim());
  protected readonly markup = computed<SafeHtml>(() =>
    this.sanitizer.bypassSecurityTrustHtml(WU_ICONS[this.name()] ?? ''),
  );
}
