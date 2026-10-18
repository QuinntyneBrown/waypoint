import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { SkipLink } from './skip-link';

@Component({
  imports: [SkipLink],
  template: `<a w-skip-link href="#main">Skip to content</a>`,
})
class Host {}

describe('SkipLink', () => {
  it('is a native link to the main content', async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const link = (fixture.nativeElement as HTMLElement).querySelector('a')!;
    expect(link.classList.contains('w-skip-link')).toBe(true);
    expect(link.getAttribute('href')).toBe('#main');
    expect(link.textContent).toBe('Skip to content');
  });
});
