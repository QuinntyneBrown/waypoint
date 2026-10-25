import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Icon } from './icon';

@Component({
  imports: [Icon],
  template: `<ph-icon [name]="n" [iconClass]="c" />`,
})
class Host {
  n = 'home';
  c = '';
}

describe('Icon', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    return fixture;
  }

  it('renders an inline svg with the ph-icon class', async () => {
    const fixture = await render();
    const svg = fixture.nativeElement.querySelector('svg');
    expect(svg).toBeTruthy();
    expect(svg.classList.contains('ph-icon')).toBe(true);
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
    fixture.componentInstance.c = 'ph-alert__icon';
    await fixture.whenStable();
    const svg = fixture.nativeElement.querySelector('svg');
    expect(svg.classList.contains('ph-icon')).toBe(true);
    expect(svg.classList.contains('ph-alert__icon')).toBe(true);
  });
});
