import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { WIcon } from './w-icon';

@Component({
  imports: [WIcon],
  template: `<w-icon [name]="n" [iconClass]="c" />`,
})
class Host {
  n = 'home';
  c = '';
}

describe('WIcon', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    return fixture;
  }

  it('renders an inline svg with the w-icon class', async () => {
    const fixture = await render();
    const svg = fixture.nativeElement.querySelector('svg');
    expect(svg).toBeTruthy();
    expect(svg.classList.contains('w-icon')).toBe(true);
    expect(svg.getAttribute('aria-hidden')).toBe('true');
  });

  it('renders the named icon path markup as svg children', async () => {
    const fixture = await render();
    // 'home' is a <path> + <polyline>; assert the glyph markup made it into the DOM.
    const svg = fixture.nativeElement.querySelector('svg');
    expect(svg.querySelector('path')).toBeTruthy();
    expect(svg.querySelector('polyline')).toBeTruthy();
  });

  it('merges an extra iconClass onto the svg', async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    fixture.componentInstance.c = 'w-alert__icon';
    await fixture.whenStable();
    const svg = fixture.nativeElement.querySelector('svg');
    expect(svg.classList.contains('w-icon')).toBe(true);
    expect(svg.classList.contains('w-alert__icon')).toBe(true);
  });
});
