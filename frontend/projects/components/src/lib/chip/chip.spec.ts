import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Chip } from './chip';
import { ChipRow } from './chip-row';

@Component({
  imports: [Chip, ChipRow],
  template: `
    <div w-chip-row aria-label="Roster status filter">
      <button w-chip type="button" [active]="filter() === 'all'">All</button>
      <button w-chip type="button" [active]="filter() === 'at-risk'">At risk</button>
    </div>
  `,
})
class Host {
  readonly filter = signal('all');
}

describe('Chip', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const chips = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll('button'),
    ) as HTMLButtonElement[];
    return { fixture, chips };
  }

  it('lays chips out in a row of native buttons', async () => {
    const { fixture, chips } = await render();
    expect(fixture.nativeElement.querySelector('div')?.classList.contains('w-pillrow')).toBe(true);
    expect(chips.map((c) => c.textContent?.trim())).toEqual(['All', 'At risk']);
    expect(chips[0].classList.contains('w-pill')).toBe(true);
  });

  it('marks only the active chip and reports it as pressed', async () => {
    const { fixture, chips } = await render();
    expect(chips.map((c) => c.classList.contains('is-active'))).toEqual([true, false]);
    expect(chips.map((c) => c.getAttribute('aria-pressed'))).toEqual(['true', 'false']);

    fixture.componentInstance.filter.set('at-risk');
    await fixture.whenStable();
    expect(chips.map((c) => c.classList.contains('is-active'))).toEqual([false, true]);
    expect(chips.map((c) => c.getAttribute('aria-pressed'))).toEqual(['false', 'true']);
  });
});
