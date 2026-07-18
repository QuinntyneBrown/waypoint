import { ChangeDetectionStrategy, Component, computed, signal } from '@angular/core';

// Traces to: L2-PUB-003
@Component({
  selector: 'wu-donate',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './donate.html',
})
export class Donate {
  protected readonly tiers = [20, 50, 100] as const;
  protected readonly selected = signal<number | null>(null);
  protected readonly confirmed = signal(false);

  /**
   * Handoff URL to the external PCI-compliant provider. No card data is ever
   * collected or stored by this app (L2-PUB-003 AC3).
   */
  protected readonly providerUrl = computed(
    () =>
      `https://donate.example-pci-provider.org/word-up?interval=monthly&amount=${this.selected() ?? ''}`,
  );

  protected select(amount: number): void {
    this.selected.set(amount);
    this.confirmed.set(false);
  }

  protected confirm(): void {
    if (this.selected() !== null) this.confirmed.set(true);
  }
}
