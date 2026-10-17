import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { WBrand } from './w-brand';

@Component({
  imports: [WBrand],
  template: `
    <a w-brand href="/">Waypoint</a>
    <div w-brand size="lg">Waypoint</div>
  `,
})
class Host {}

describe('WBrand', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    return Array.from((fixture.nativeElement as HTMLElement).children) as HTMLElement[];
  }

  it('draws the mark and the name, as a link or a plain block', async () => {
    const [link, block] = await render();
    expect(link.tagName).toBe('A');
    expect(link.getAttribute('href')).toBe('/');
    expect(link.classList.contains('w-brand')).toBe(true);
    expect(link.querySelector('.w-brand__mark')?.textContent).toBe('W');
    expect(link.querySelector('.w-brand__name')?.textContent).toBe('Waypoint');
    expect(block.querySelector('.w-brand__mark')).not.toBeNull();
  });

  it('has a large size for sign-in cards', async () => {
    const [link, block] = await render();
    expect(link.classList.contains('w-brand--lg')).toBe(false);
    expect(block.classList.contains('w-brand--lg')).toBe(true);
  });
});
