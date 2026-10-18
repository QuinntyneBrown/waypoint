import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  computed,
  input,
} from '@angular/core';

export type AvatarSize = 'xs' | 'sm' | 'lg' | 'xl';

/** 8-colour accent palette for avatar backgrounds (mirrors `waypoint.js` AV_BG). */
export const W_AVATAR_PALETTE = [
  '#c6fb50',
  '#8adfff',
  '#ffd700',
  '#ffb300',
  '#bae386',
  '#e4e0d8',
  '#f05228',
  '#1d8fb9',
];

export function wuInitials(name: string): string {
  const parts = (name || '?').trim().split(/\s+/);
  return ((parts[0]?.[0] ?? '') + (parts[1]?.[0] ?? '')).toUpperCase();
}

/** Deterministic palette index — same hash the mock uses so colours stay stable. */
export function wuAvatarColor(name: string): string {
  const s = name || 'x';
  let h = 0;
  for (let i = 0; i < s.length; i++) h = (h * 31 + s.charCodeAt(i)) % 100000;
  return W_AVATAR_PALETTE[h % W_AVATAR_PALETTE.length];
}

/**
 * Initials avatar with a deterministic colour derived from the name. Ink text
 * keeps contrast on the light accent palette. Decorative — the surrounding
 * context supplies the accessible name, so the glyph is aria-hidden.
 */
@Component({
  selector: 'w-avatar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'w-avatar',
    '[class.w-avatar--xs]': "size() === 'xs'",
    '[class.w-avatar--sm]': "size() === 'sm'",
    '[class.w-avatar--lg]': "size() === 'lg'",
    '[class.w-avatar--xl]': "size() === 'xl'",
    '[style.background]': 'bg()',
    '[style.color]': "'#16160c'",
    '[attr.aria-hidden]': "'true'",
  },
  template: `{{ initials() }}`,
  styles: `
    .w-avatar {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      flex: none;
      display: inline-grid;
      place-items: center;
      font-family: var(--w-font-head);
      font-weight: 500;
      font-size: 0.9rem;
      color: var(--w-ink);
      background: var(--w-g300);
      overflow: hidden;
      letter-spacing: 0;
      text-transform: uppercase;
    }
    .w-avatar--xs {
      width: 26px;
      height: 26px;
      font-size: 0.66rem;
    }
    .w-avatar--sm {
      width: 32px;
      height: 32px;
      font-size: 0.75rem;
    }
    .w-avatar--lg {
      width: 56px;
      height: 56px;
      font-size: 1.2rem;
    }
    .w-avatar--xl {
      width: 88px;
      height: 88px;
      font-size: 1.9rem;
    }
  `,
})
export class Avatar {
  readonly name = input.required<string>();
  readonly size = input<AvatarSize | undefined>(undefined);

  protected readonly initials = computed(() => wuInitials(this.name()));
  protected readonly bg = computed(() => wuAvatarColor(this.name()));
}
