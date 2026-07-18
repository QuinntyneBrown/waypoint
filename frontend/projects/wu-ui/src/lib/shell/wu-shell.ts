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
import { WuAvatar } from '../avatar/wu-avatar';
import { WuIcon } from '../icon/wu-icon';
import { OfflineService } from '../offline/offline.service';
import { isNavGroup, WU_NAV, WU_ROLE_LABEL, WuNavItem, WuRole } from './nav-config';

/**
 * Authenticated app shell: fixed topbar, role-aware sidenav (+ mobile bottom
 * nav), offline banner, and an account menu. Renders what the caller says the
 * user may see — never the access authority itself (DECISIONS.md #5).
 * Page content is projected into `<main class="wu-main">`.
 */
@Component({
  selector: 'wu-shell',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, WuIcon, WuAvatar],
  host: {
    class: 'wu-shell',
    '[class.wu-has-sidenav]': 'true',
    '[class.wu-has-botnav]': 'botItems().length > 0',
    '[class.is-nav-open]': 'navOpen()',
    '[class.is-offline]': 'showOffline()',
  },
  templateUrl: './wu-shell.html',
})
export class WuShell {
  readonly role = input.required<WuRole>();
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

  protected readonly nav = computed(() => WU_NAV[this.role()]);
  protected readonly roleLabel = computed(() => WU_ROLE_LABEL[this.role()]);
  protected readonly botItems = computed<WuNavItem[]>(() => {
    const cfg = this.nav();
    return cfg.botnav
      .map((key) => cfg.items.find((i): i is WuNavItem => !isNavGroup(i) && i.key === key))
      .filter((i): i is WuNavItem => !!i);
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
