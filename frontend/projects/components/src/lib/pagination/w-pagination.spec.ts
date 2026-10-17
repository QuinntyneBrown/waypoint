import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { WPagination } from './w-pagination';

@Component({
  imports: [WPagination],
  template: `
    <nav w-pagination aria-label="People pages">
      <p>Page 1 of 3</p>
      <button type="button">Next</button>
    </nav>
  `,
})
class Host {}

describe('WPagination', () => {
  it('is a navigation landmark that keeps its label and content', async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const nav = (fixture.nativeElement as HTMLElement).querySelector('nav')!;
    expect(nav.classList.contains('w-pagination')).toBe(true);
    expect(nav.getAttribute('aria-label')).toBe('People pages');
    expect(nav.textContent).toContain('Page 1 of 3');
    expect(nav.querySelector('button')?.textContent).toBe('Next');
  });
});
