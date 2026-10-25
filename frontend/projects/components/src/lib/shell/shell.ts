import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { Avatar } from '../avatar/avatar';
import { Brand } from '../brand/brand';
import { Icon } from '../icon/icon';
import { OfflineService } from '../offline/offline.service';
import { isNavGroup, W_NAV, W_ROLE_LABEL, NavItem, Role } from './nav-config';
import { IconButton } from '../icon-button/icon-button';
import { SkipLink } from '../skip-link/skip-link';

/**
 * Authenticated app shell: fixed topbar, role-aware sidenav (+ mobile bottom
 * nav), offline banner, and an account menu. Renders what the caller says the
 * user may see — never the access authority itself (DECISIONS.md #5).
 * Page content is projected into `<main class="ph-main">`.
 */
@Component({
  selector: 'ph-shell',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, Icon, Avatar, IconButton, Brand, SkipLink],
  host: {
    class: 'ph-shell',
    // `role` is the product-role input name, not an ARIA role. Static Angular
    // inputs remain as host attributes unless explicitly removed.
    '[attr.role]': 'null',
    '[class.ph-has-sidenav]': 'true',
    '[class.ph-has-botnav]': 'botItems().length > 0',
    '[class.is-nav-open]': 'navOpen()',
    '[class.is-offline]': 'showOffline()',
  },
  templateUrl: './shell.html',
  // Visually hidden name, so the visible avatar initials don't contradict an aria-label.
  styleUrl: './shell.css',
})
export class Shell {
  readonly role = input.required<Role>();
  /** Active nav key for highlighting. */
  readonly active = input('');
  /** Display name for the account affordance. */
  readonly userName = input('');
  /** Manual override; the live connectivity state is added automatically. */
  readonly offline = input(false);
  readonly signOut = output<void>();

  private readonly offlineService = inject(OfflineService);
  protected readonly showOffline = computed(
    () => this.offline() || this.offlineService.isOffline(),
  );

  protected readonly navOpen = signal(false);
  protected readonly accountOpen = signal(false);

  protected readonly nav = computed(() => W_NAV[this.role()]);
  protected readonly roleLabel = computed(() => W_ROLE_LABEL[this.role()]);
  protected readonly botItems = computed<NavItem[]>(() => {
    const cfg = this.nav();
    return cfg.botnav
      .map((key) => cfg.items.find((i): i is NavItem => !isNavGroup(i) && i.key === key))
      .filter((i): i is NavItem => !!i);
  });

  protected isGroup = isNavGroup;

  protected toggleNav(): void {
    this.navOpen.update((v) => !v);
  }
  protected closeNav(): void {
    this.navOpen.set(false);
  }
  protected toggleAccount(): void {
    this.accountOpen.update((v) => !v);
  }
  protected emitSignOut(): void {
    this.accountOpen.set(false);
    this.signOut.emit();
  }
}
