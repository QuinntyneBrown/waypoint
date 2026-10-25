import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  computed,
  input,
} from '@angular/core';

export type AvatarSize = 'xs' | 'sm' | 'lg' | 'xl';

/** PromiseHub pastel covers for avatar backgrounds (mirrors `waypoint.js` AV_BG). */
export const W_AVATAR_PALETTE = ['sage', 'rose', 'sky', 'sun', 'lilac', 'mint', 'peach'].map(
  (cover) => `var(--color-cover-${cover})`,
);

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
 * Initials avatar with a deterministic colour derived from the name. The
 * on-cover text token keeps contrast on every pastel cover in both themes. Decorative — the surrounding
 * context supplies the accessible name, so the glyph is aria-hidden.
 */
@Component({
  selector: 'ph-avatar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'ph-avatar',
    '[class.ph-avatar--xs]': "size() === 'xs'",
    '[class.ph-avatar--sm]': "size() === 'sm'",
    '[class.ph-avatar--lg]': "size() === 'lg'",
    '[class.ph-avatar--xl]': "size() === 'xl'",
    '[style.background]': 'bg()',
    '[style.color]': "'var(--color-fg-on-cover)'",
    '[attr.aria-hidden]': "'true'",
  },
  templateUrl: './avatar.html',
  styleUrl: './avatar.css',
})
export class Avatar {
  readonly name = input.required<string>();
  readonly size = input<AvatarSize | undefined>(undefined);

  protected readonly initials = computed(() => wuInitials(this.name()));
  protected readonly bg = computed(() => wuAvatarColor(this.name()));
}
