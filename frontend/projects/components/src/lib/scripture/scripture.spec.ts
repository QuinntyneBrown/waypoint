import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Scripture } from './scripture';
import { ScriptureRef } from './scripture-ref';

@Component({
  imports: [Scripture, ScriptureRef],
  template: `
    <div ph-scripture>“Serve one another.” <span ph-scripture-ref>Galatians 5:13</span></div>
    <p ph-scripture>Think about it</p>
  `,
})
class Host {}

describe('Scripture', () => {
  it('quotes a passage with an optional reference', async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const [quote, prompt] = Array.from((fixture.nativeElement as HTMLElement).children);
    expect(quote.classList.contains('ph-scripture')).toBe(true);
    expect(quote.querySelector('.ph-scripture__ref')?.textContent).toBe('Galatians 5:13');
    expect(prompt.classList.contains('ph-scripture')).toBe(true);
  });
});
