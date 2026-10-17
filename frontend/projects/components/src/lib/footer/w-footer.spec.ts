import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { WFooter } from './w-footer';

@Component({
  imports: [WFooter],
  template: `<footer w-footer><p>© 2026 Waypoint</p></footer>`,
})
class Host {}

describe('WFooter', () => {
  it('is the page footer landmark and keeps its content', async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const footer = (fixture.nativeElement as HTMLElement).querySelector('footer')!;
    expect(footer.classList.contains('w-footer')).toBe(true);
    expect(footer.textContent).toBe('© 2026 Waypoint');
  });
});
