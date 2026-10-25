import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { IconButton } from './icon-button';

@Component({
  imports: [IconButton],
  template: `
    <button ph-icon-button type="button" aria-label="Close">x</button>
    <a ph-icon-button href="/admin/people/1" aria-label="Open Priya Sharma">o</a>
  `,
})
class Host {}

describe('IconButton', () => {
  it('styles a button and keeps its label and content', async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const button = (fixture.nativeElement as HTMLElement).querySelector('button')!;
    expect(button.classList.contains('ph-iconbtn')).toBe(true);
    expect(button.getAttribute('aria-label')).toBe('Close');
    expect(button.textContent?.trim()).toBe('x');
  });

  it('styles a link the same way', async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const link = (fixture.nativeElement as HTMLElement).querySelector('a')!;
    expect(link.classList.contains('ph-iconbtn')).toBe(true);
    expect(link.getAttribute('href')).toBe('/admin/people/1');
  });
});
