import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ButtonGroup } from './button-group';
import { ButtonGroupItem } from './button-group-item';

@Component({
  imports: [ButtonGroup, ButtonGroupItem],
  template: `
    <div ph-button-group role="group" aria-label="Theme">
      <button ph-button-group-item type="button" [active]="theme() === 'light'">Light</button>
      <button ph-button-group-item type="button" [active]="theme() === 'dark'">Dark</button>
    </div>
  `,
})
class Host {
  readonly theme = signal('light');
}

describe('ButtonGroup', () => {
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
    expect(fixture.nativeElement.querySelector('div')?.classList.contains('ph-segment')).toBe(true);
    expect(items.map((i) => i.textContent?.trim())).toEqual(['Light', 'Dark']);
    expect(items[0].classList.contains('ph-segment__item')).toBe(true);
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
