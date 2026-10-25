import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Footer } from './footer';

@Component({
  imports: [Footer],
  template: `<footer ph-footer><p>© 2026 Waypoint</p></footer>`,
})
class Host {}

describe('Footer', () => {
  it('is the page footer landmark and keeps its content', async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const footer = (fixture.nativeElement as HTMLElement).querySelector('footer')!;
    expect(footer.classList.contains('ph-footer')).toBe(true);
    expect(footer.textContent).toBe('© 2026 Waypoint');
  });
});
