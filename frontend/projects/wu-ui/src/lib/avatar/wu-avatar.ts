import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

export type WuAvatarSize = 'xs' | 'sm' | 'lg' | 'xl';

/** 8-colour accent palette for avatar backgrounds (mirrors `wordup.js` AV_BG). */
export const WU_AVATAR_PALETTE = [
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
  return WU_AVATAR_PALETTE[h % WU_AVATAR_PALETTE.length];
}

/**
 * Initials avatar with a deterministic colour derived from the name. Ink text
 * keeps contrast on the light accent palette. Decorative — the surrounding
 * context supplies the accessible name, so the glyph is aria-hidden.
 */
@Component({
  selector: 'wu-avatar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'wu-avatar',
    '[class.wu-avatar--xs]': "size() === 'xs'",
    '[class.wu-avatar--sm]': "size() === 'sm'",
    '[class.wu-avatar--lg]': "size() === 'lg'",
    '[class.wu-avatar--xl]': "size() === 'xl'",
    '[style.background]': 'bg()',
    '[style.color]': "'#16160c'",
    '[attr.aria-hidden]': "'true'",
  },
  template: `{{ initials() }}`,
})
export class WuAvatar {
  readonly name = input.required<string>();
  readonly size = input<WuAvatarSize | undefined>(undefined);

  protected readonly initials = computed(() => wuInitials(this.name()));
  protected readonly bg = computed(() => wuAvatarColor(this.name()));
}
