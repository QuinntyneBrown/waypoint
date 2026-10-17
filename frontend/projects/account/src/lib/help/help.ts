import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthStore } from '@waypoint/auth';
import { WButton, WIcon, WShell } from '@waypoint/components';
import { shellRole } from '@waypoint/core';
import { helpTopicsFor } from './help-content';

// Traces to: L2-PLATFORM-005
@Component({
  selector: 'w-help',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, WIcon, WShell, WButton],
  templateUrl: './help.html',
  styles: `
    .help-search {
      max-width: 44rem;
      position: relative;
    }
    .help-search__icon {
      left: var(--w-s4);
      display: grid;
      pointer-events: none;
      position: absolute;
      top: 50%;
      transform: translateY(-50%);
    }
    .help-search input {
      padding-left: 3rem;
    }
    .help-grid {
      display: grid;
      gap: var(--w-s5);
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }
    .help-topic__title {
      align-items: center;
      display: flex;
      gap: var(--w-s3);
    }
    .help-answer {
      border-top: 1px solid var(--w-border);
      padding: var(--w-s4) 0;
    }
    .help-answer summary {
      cursor: pointer;
      font-weight: 700;
      min-height: 44px;
      padding: var(--w-s2) 0;
    }
    .help-answer p {
      margin: var(--w-s2) 0 0;
    }
    @media (max-width: 48rem) {
      .help-grid {
        grid-template-columns: 1fr;
      }
    }
  `,
})
export class HelpPage {
  protected readonly auth = inject(AuthStore);
  protected readonly role = computed(() => shellRole(this.auth.role()));
  protected readonly query = signal('');
  protected readonly topics = computed(() => helpTopicsFor(this.role(), this.query()));

  protected search(event: Event): void {
    this.query.set((event.target as HTMLInputElement).value);
  }
}
