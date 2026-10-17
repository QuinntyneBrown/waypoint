import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { WScripture, WScriptureRef } from './w-scripture';

@Component({
  imports: [WScripture, WScriptureRef],
  template: `
    <div w-scripture>“Serve one another.” <span w-scripture-ref>Galatians 5:13</span></div>
    <p w-scripture>Think about it</p>
  `,
})
class Host {}

describe('WScripture', () => {
  it('quotes a passage with an optional reference', async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const [quote, prompt] = Array.from((fixture.nativeElement as HTMLElement).children);
    expect(quote.classList.contains('w-scripture')).toBe(true);
    expect(quote.querySelector('.w-scripture__ref')?.textContent).toBe('Galatians 5:13');
    expect(prompt.classList.contains('w-scripture')).toBe(true);
  });
});
