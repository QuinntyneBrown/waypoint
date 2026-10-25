import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Textarea } from './textarea';

@Component({ imports: [Textarea], template: `<textarea ph-textarea rows="4"></textarea>` })
class Host {}

describe('Textarea', () => {
  it('styles the native textarea with the ph-textarea class and keeps its attributes', async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const el = (fixture.nativeElement as HTMLElement).querySelector('textarea')!;
    expect(el.classList.contains('ph-textarea')).toBe(true);
    expect(el.hasAttribute('ph-textarea')).toBe(true);
  });
});
