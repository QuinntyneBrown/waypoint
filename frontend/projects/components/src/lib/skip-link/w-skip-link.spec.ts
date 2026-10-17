import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { WSkipLink } from './w-skip-link';

@Component({
  imports: [WSkipLink],
  template: `<a w-skip-link href="#main">Skip to content</a>`,
})
class Host {}

describe('WSkipLink', () => {
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
