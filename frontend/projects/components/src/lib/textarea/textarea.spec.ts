import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Textarea } from './textarea';

@Component({ imports: [Textarea], template: `<textarea w-textarea rows="4"></textarea>` })
class Host {}

describe('Textarea', () => {
  it('styles the native textarea with the w-textarea class and keeps its attributes', async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const el = (fixture.nativeElement as HTMLElement).querySelector('textarea')!;
    expect(el.classList.contains('w-textarea')).toBe(true);
    expect(el.hasAttribute('w-textarea')).toBe(true);
  });
});
