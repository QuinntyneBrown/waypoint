import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { WTab, WTabs } from './w-tabs';

@Component({
  imports: [WTabs, WTab],
  template: `
    <div w-tabs role="tablist" aria-label="Report views">
      <button w-tab type="button" role="tab" [active]="view() === 'overview'">Overview</button>
      <button w-tab type="button" role="tab" [active]="view() === 'youth'">Youth</button>
    </div>
    <nav w-tabs aria-label="Settings">
      <a w-tab href="/settings/profile" [active]="true">Profile</a>
    </nav>
  `,
})
class Host {
  readonly view = signal('overview');
}

describe('WTabs', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const root = fixture.nativeElement as HTMLElement;
    return { fixture, root, tabs: Array.from(root.querySelectorAll('button')) };
  }

  it('lays tabs out in a row and keeps the tablist semantics', async () => {
    const { root } = await render();
    const list = root.querySelector('div')!;
    expect(list.classList.contains('w-tabs')).toBe(true);
    expect(list.getAttribute('role')).toBe('tablist');
    expect(root.querySelector('nav')?.classList.contains('w-tabs')).toBe(true);
  });

  it('marks only the active tab and follows changes', async () => {
    const { fixture, tabs } = await render();
    expect(tabs.map((t) => t.classList.contains('is-active'))).toEqual([true, false]);
    fixture.componentInstance.view.set('youth');
    await fixture.whenStable();
    expect(tabs.map((t) => t.classList.contains('is-active'))).toEqual([false, true]);
    expect(tabs[0].classList.contains('w-tab')).toBe(true);
  });

  it('works on links for navigation tabs', async () => {
    const { root } = await render();
    const link = root.querySelector('a')!;
    expect(link.classList.contains('w-tab')).toBe(true);
    expect(link.classList.contains('is-active')).toBe(true);
  });
});
