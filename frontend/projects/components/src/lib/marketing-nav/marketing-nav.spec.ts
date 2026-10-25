import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { MarketingNav } from './marketing-nav';
import { MarketingNavLinks } from './marketing-nav-links';

@Component({
  imports: [MarketingNav, MarketingNavLinks],
  template: `
    <header ph-marketing-nav>
      <span id="brand">Waypoint</span>
      <nav ph-marketing-nav-links aria-label="Primary"><a href="/about">About</a></nav>
      <button type="button">Apply</button>
    </header>
  `,
})
class Host {}

describe('MarketingNav', () => {
  it('frames a brand, links and actions in a contained bar', async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const header = (fixture.nativeElement as HTMLElement).querySelector('header')!;
    expect(header.classList.contains('ph-marketing-nav')).toBe(true);
    const inner = header.querySelector('.ph-marketing-nav__inner')!;
    expect(inner.classList.contains('ph-container')).toBe(true);
    expect(inner.querySelector('#brand')).not.toBeNull();
    expect(inner.querySelector('button')?.textContent).toBe('Apply');
    const links = inner.querySelector('nav')!;
    expect(links.classList.contains('ph-marketing-nav__links')).toBe(true);
    expect(links.getAttribute('aria-label')).toBe('Primary');
  });
});
