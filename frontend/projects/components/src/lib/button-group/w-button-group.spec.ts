import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { WButtonGroup, WButtonGroupItem } from './w-button-group';

@Component({
  imports: [WButtonGroup, WButtonGroupItem],
  template: `
    <div w-button-group role="group" aria-label="Theme">
      <button w-button-group-item type="button" [active]="theme() === 'light'">Light</button>
      <button w-button-group-item type="button" [active]="theme() === 'dark'">Dark</button>
    </div>
  `,
})
class Host {
  readonly theme = signal('light');
}

describe('WButtonGroup', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const items = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll('button'),
    ) as HTMLButtonElement[];
    return { fixture, items };
  }

  it('is a segmented group of native buttons', async () => {
    const { fixture, items } = await render();
    expect(fixture.nativeElement.querySelector('div')?.classList.contains('w-segment')).toBe(true);
    expect(items.map((i) => i.textContent?.trim())).toEqual(['Light', 'Dark']);
    expect(items[0].classList.contains('w-segment__item')).toBe(true);
  });

  it('marks only the active item and reports it as pressed', async () => {
    const { fixture, items } = await render();
    expect(items.map((i) => i.classList.contains('is-active'))).toEqual([true, false]);
    expect(items.map((i) => i.getAttribute('aria-pressed'))).toEqual(['true', 'false']);

    fixture.componentInstance.theme.set('dark');
    await fixture.whenStable();
    expect(items.map((i) => i.classList.contains('is-active'))).toEqual([false, true]);
    expect(items.map((i) => i.getAttribute('aria-pressed'))).toEqual(['false', 'true']);
  });
});
